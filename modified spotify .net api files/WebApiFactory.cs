﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace SpotifyAPI.Web.Auth
{
    public class WebAPIFactory
    {
        private readonly string _redirectUrl;
        private readonly int _listeningPort;
        private readonly string _clientId;
        private readonly TimeSpan _timeout;
        private readonly Scope _scope;
        private readonly string _xss;
        public ImplicitGrantAuth authentication;

        public WebAPIFactory(string redirectUrl, int listeningPort, string clientId, Scope scope)
            : this(redirectUrl, listeningPort, clientId, scope, TimeSpan.FromSeconds(20))
        {
        }

        public WebAPIFactory(string redirectUrl, int listeningPort, string clientId, Scope scope, TimeSpan timeout, string xss = "XSS")
        {
            _redirectUrl = redirectUrl;
            _listeningPort = listeningPort;
            _clientId = clientId;
            _scope = scope;
            _timeout = timeout;
            _xss = xss;
        }

        public Task<SpotifyWebAPI> GetWebApi()
        {
            authentication = authentication?? new ImplicitGrantAuth
            {
                RedirectUri = new UriBuilder(_redirectUrl) { Port = _listeningPort }.Uri.OriginalString.TrimEnd('/'),
                ClientId = _clientId,
                Scope = _scope,
                State = _xss
            };

            AutoResetEvent authenticationWaitFlag = new AutoResetEvent(false);
            SpotifyWebAPI spotifyWebApi = null;
            SpotifyWebApiException spotifyWebApiException = null;
            authentication.OnResponseReceivedEvent += (token, state) =>
            {
                try
                {
                    spotifyWebApi = HandleSpotifyResponse(state, token);
                }
                catch (SpotifyWebApiException s)
                {
                    spotifyWebApiException = s;
                }
                authenticationWaitFlag.Set();
            };

            try
            {
                authentication.StartHttpServer(_listeningPort);

                authentication.DoAuth();
                
                authenticationWaitFlag.WaitOne(_timeout);
                if (spotifyWebApiException != null)
                    throw new SpotifyWebApiException(spotifyWebApiException.Message);
                if (spotifyWebApi == null)
                    throw new TimeoutException($"No valid response received for the last {_timeout.TotalSeconds} seconds");
            }
            finally
            {
                authentication.StopHttpServer();
            }

            return Task.FromResult(spotifyWebApi);
        }

        private SpotifyWebAPI HandleSpotifyResponse(string state, Token token)
        {
            if (state != _xss)
                throw new SpotifyWebApiException($"Wrong state '{state}' received.");

            if (token.Error != null)
                throw new SpotifyWebApiException($"Error: {token.Error}");

            var spotifyWebApi = new SpotifyWebAPI
            {
                UseAuth = true,
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };

            return spotifyWebApi;
        }
    }

    [Serializable]
    public class SpotifyWebApiException : Exception
    {
        public SpotifyWebApiException(string message) : base(message)
        { }
    }
}
