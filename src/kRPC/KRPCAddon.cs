﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;
using KSP.IO;
using KRPC.Server;
using KRPC.Server.Net;
using KRPC.Server.RPC;
using KRPC.Service;
using KRPC.Schema.KRPC;
using KRPC.Utils;
using KRPC.UI;

namespace KRPC
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    sealed public class KRPCAddon : MonoBehaviour
    {
        private KRPCServer server;
        private KRPCConfiguration config;
        private IButton toolbarButton;
        private MainWindow mainWindow;
        private ClientConnectingDialog clientConnectingDialog;

        public void Awake ()
        {
            //TODO: fails due to native code not being available
//            Logger.WriteLine ("Local addresses of available network adapters:");
//            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces()) {
//                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses) {
//                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork) {
//                        Logger.WriteLine ("   " + unicastIPAddressInformation.Address.ToString());
//                    }
//                }
//            }

            config = new KRPCConfiguration ("settings.cfg");
            config.Load ();
            server = new KRPCServer (config.Address, config.Port);

            // Create main window
            mainWindow = gameObject.AddComponent<MainWindow>();
            mainWindow.Config = config;
            mainWindow.Server = server;
            mainWindow.Visible = config.MainWindowVisible;
            mainWindow.Position = config.MainWindowPosition;

            // Create new connection dialog
            clientConnectingDialog = gameObject.AddComponent<ClientConnectingDialog>();

            // Main window events
            mainWindow.OnStartServerPressed += (s, e) => {
                server.Port = config.Port;
                server.Address = config.Address;
                try {
                    server.Start ();
                } catch (SocketException exn) {
                    mainWindow.Errors.Add ("Socket error '" + exn.SocketErrorCode + "': " + exn.Message);
                }
            };
            mainWindow.OnStopServerPressed += (s, e) => {
                server.Stop ();
                clientConnectingDialog.Close();
            };
            mainWindow.OnHide += (s, e) => {
                config.MainWindowVisible = false;
                config.Save ();
            };
            mainWindow.OnShow += (s, e) => {
                config.MainWindowVisible = true;
                config.Save ();
            };
            mainWindow.OnMoved += (s, e) => {
                var window = s as MainWindow;
                config.MainWindowPosition = window.Position;
                config.Save ();
            };

            // Server events
            server.OnClientRequestingConnection += clientConnectingDialog.OnClientRequestingConnection;
            server.OnClientActivity += (s, e) => mainWindow.SawClientActivity (e.Client);

            // Toolbar API
            if (ToolbarManager.ToolbarAvailable) {
                toolbarButton = ToolbarManager.Instance.add ("kRPC", "ToggleMainWindow");
                toolbarButton.TexturePath = "kRPC/icon-offline";
                toolbarButton.ToolTip = "kRPC Server";
                toolbarButton.Visibility = new GameScenesVisibility (GameScenes.FLIGHT);
                toolbarButton.OnClick += (e) => {
                    mainWindow.Visible = !mainWindow.Visible;
                };
            } else {
                // If there is no toolbar button a hidden window can't be shown, so force it to be displayed
                mainWindow.Visible = true;
            }
        }

        public void OnDestroy ()
        {
            if (server.Running)
                server.Stop ();
            toolbarButton.Destroy ();
            UnityEngine.Object.Destroy (mainWindow);
            UnityEngine.Object.Destroy (clientConnectingDialog);
        }

        public void Update ()
        {
            // TODO: add server start/stop events to IServer and attach these updates to the handlers
            if (toolbarButton != null) {
                if (server.Running)
                    toolbarButton.TexturePath = "kRPC/icon-online";
                else
                    toolbarButton.TexturePath = "kRPC/icon-offline";
            }
            if (server.Running)
                server.Update ();
        }
    }
}
