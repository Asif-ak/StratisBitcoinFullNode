﻿using System;
using System.Net;
using NBitcoin.Protocol;
using Newtonsoft.Json;
using Stratis.Bitcoin.Utilities.JsonConverters;

namespace Stratis.Bitcoin.P2P
{
    /// <summary>
    /// A class which holds data on a peer's (IPEndPoint) attempts, connections and successful handshake events.
    /// </summary>
    [JsonObject]
    public sealed class PeerAddress
    {
        /// <summary>EndPoint of this peer.</summary>
        [JsonProperty(PropertyName = "endpoint")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint EndPoint { get; set; }

        /// <summary>Used to construct the <see cref="NetworkAddress"/> after deserializing this peer.</summary>
        [JsonProperty(PropertyName = "addressTime", NullValueHandling = NullValueHandling.Ignore)]
        private DateTimeOffset? addressTime;

        /// <summary>The source address of this peer.</summary>
        [JsonProperty(PropertyName = "loopback")]
        private string loopback;

        [JsonIgnore]
        public IPAddress Loopback
        {
            get
            {
                if (string.IsNullOrEmpty(this.loopback))
                    return null;
                return IPAddress.Parse(this.loopback);
            }
        }

        /// <summary>
        /// The amount of connection attempts.
        /// <para>
        /// This gets reset when a connection was successful.</para>
        /// </summary>
        [JsonProperty(PropertyName = "connectionAttempts")]
        public int ConnectionAttempts { get; private set; }

        /// <summary>
        /// The last successful version handshake.
        /// <para>
        /// This is set when the connection attempt was successful and a handshake was done.
        /// </para>
        /// </summary>
        [JsonProperty(PropertyName = "lastConnectionHandshake", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastConnectionHandshake { get; private set; }

        /// <summary>
        /// The last time this peer was seen.
        /// <para>
        /// This is set via <see cref="Protocol.Behaviors.PingPongBehavior"/> to ensure that a peer is live.
        /// </para>
        /// </summary>
        [JsonProperty(PropertyName = "lastSeen", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastSeen { get; private set; }

        /// <summary>
        /// <c>True</c> if the peer has had connection attempts but none successful.
        /// </summary>
        [JsonIgnore]
        public bool Attempted
        {
            get
            {
                return
                    (this.LastConnectionAttempt != null) &&
                    (this.LastConnectionSuccess == null) &&
                    (this.LastConnectionHandshake == null);
            }
        }

        /// <summary>
        /// <c>True</c> if the peer has had a successful connection attempt.
        /// </summary>
        [JsonIgnore]
        public bool Connected
        {
            get
            {
                return
                    (this.LastConnectionAttempt == null) &&
                    (this.LastConnectionSuccess != null) &&
                    (this.LastConnectionHandshake == null);
            }
        }

        /// <summary>
        /// <c>True</c> if the peer has never had connection attempts.
        /// </summary>
        [JsonIgnore]
        public bool Fresh
        {
            get
            {
                return
                    (this.LastConnectionAttempt == null) &&
                    (this.LastConnectionSuccess == null) &&
                    (this.LastConnectionHandshake == null);
            }
        }

        /// <summary>
        /// <c>True</c> if the peer has had a successful connection attempt and handshaked.
        /// </summary>
        [JsonIgnore]
        public bool Handshaked
        {
            get
            {
                return
                    (this.LastConnectionAttempt == null) &&
                    (this.LastConnectionSuccess != null) &&
                    (this.LastConnectionHandshake != null);
            }
        }

        /// <summary>
        /// The last connection attempt.
        /// <para>
        /// This is set regardless of whether or not the connection attempt was successful or not.
        /// </para>
        /// </summary>
        [JsonProperty(PropertyName = "lastConnectionAttempt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastConnectionAttempt { get; private set; }

        /// <summary>
        /// The last successful connection attempt.
        /// <para>
        /// This is set when the connection attempt was successful (but not necessarily handshaked).
        /// </para>
        /// </summary>
        [JsonProperty(PropertyName = "lastConnectionSuccess", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastConnectionSuccess { get; private set; }

        /// <summary>
        /// Increments <see cref="ConnectionAttempts"/> and sets the <see cref="LastConnectionAttempt"/>.
        /// </summary>
        internal void SetAttempted(DateTimeOffset peerAttemptedAt)
        {
            this.ConnectionAttempts += 1;
            this.LastConnectionAttempt = peerAttemptedAt;
            this.LastConnectionSuccess = null;
            this.LastConnectionHandshake = null;
        }

        /// <summary>
        /// Sets the <see cref="LastConnectionSuccess"/>, <see cref="addressTime"/> and <see cref="NetworkAddress.Time"/> properties.
        /// <para>
        /// Resets <see cref="ConnectionAttempts"/> and <see cref="LastConnectionAttempt"/>.
        /// </para>
        /// </summary>
        internal void SetConnected(DateTimeOffset peerConnectedAt)
        {
            this.addressTime = peerConnectedAt;

            this.LastConnectionAttempt = null;
            this.ConnectionAttempts = 0;

            this.LastConnectionSuccess = peerConnectedAt;
        }

        /// <summary>Sets the <see cref="LastConnectionHandshake"/> date.</summary>
        internal void SetHandshaked(DateTimeOffset peerHandshakedAt)
        {
            this.LastConnectionHandshake = peerHandshakedAt;
        }

        /// <summary>Sets the <see cref="LastSeen"/> date.</summary>
        internal void SetLastSeen(DateTime lastSeenAt)
        {
            this.LastSeen = lastSeenAt;
        }

        /// <summary>
        /// Creates a new peer address instance.
        /// </summary>
        /// <param name="address">The network address of the peer.</param>
        public static PeerAddress Create(NetworkAddress address)
        {
            return new PeerAddress
            {
                ConnectionAttempts = 0,
                EndPoint = address.Endpoint,
                loopback = IPAddress.Loopback.ToString()
            };
        }

        /// <summary>
        /// Creates a new peer address instance and sets the loopback address (source).
        /// </summary>
        /// <param name="address">The network address of the peer.</param>
        /// <param name="loopback">The loopback (source) of the peer.</param>
        public static PeerAddress Create(NetworkAddress address, IPAddress loopback)
        {
            var peer = Create(address);
            peer.loopback = loopback.ToString();
            return peer;
        }
    }
}