﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ISocketLite.PCL.Model;
using WebsocketClientLite.PCL;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WebsocketLite.UWP.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainPage : Page

    {
        private IDisposable _subscribeToMessagesReceived;
        public MainPage()
        {
            this.InitializeComponent();
            StartTest();
        }
        private async void StartTest()
        {
            var websocketClient = new MessageWebSocketRx();

            _subscribeToMessagesReceived = websocketClient.ObserveTextMessagesReceived.Subscribe(
                msg =>
                {
                    var t = msg;
                });

            var cts = new CancellationTokenSource();

            cts.Token.Register(() =>
            {
                _subscribeToMessagesReceived.Dispose();
            });

            await
                websocketClient.ConnectAsync(
                    //new Uri("wss://echo.websocket.org:443"),
                    new Uri("wss://spc.1iveowl.dk:8088/ws/spc/?username=WS_usr&password=SpecialPostTjenesten_ws"),
                    cts,
                    ignoreServerCertificateErrors: true,
                    subprotocols: null,
                    tlsProtocolVersion: TlsProtocolVersion.Tls12);

            var test = "";
        }
    }
}
