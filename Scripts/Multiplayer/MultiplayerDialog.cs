using Godot;
using System;

/// <summary>
/// UI dialog for hosting or joining multiplayer sessions
/// </summary>
public partial class MultiplayerDialog : Window
{
    private TabContainer _tabContainer;
    private LineEdit _portInput;
    private SpinBox _maxPlayersInput;
    private Button _hostButton;
    private LineEdit _addressInput;
    private LineEdit _clientPortInput;
    private Button _joinButton;
    private Button _disconnectButton;
    private Label _statusLabel;
    private ItemList _playerList;

    private const int DefaultPort = 7777;
    private const int DefaultMaxPlayers = 8;

    public override void _Ready()
    {
        Title = "Multiplayer";
        Size = new Vector2I(500, 400);
        Exclusive = false;
        
        BuildUI();
        
        // Subscribe to multiplayer events
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.ServerStarted += OnServerStarted;
            MultiplayerManager.Instance.PlayerConnected += OnPlayerConnected;
            MultiplayerManager.Instance.PlayerDisconnected += OnPlayerDisconnected;
            MultiplayerManager.Instance.ConnectionFailed += OnConnectionFailed;
        }
        
        UpdateUI();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.ServerStarted -= OnServerStarted;
            MultiplayerManager.Instance.PlayerConnected -= OnPlayerConnected;
            MultiplayerManager.Instance.PlayerDisconnected -= OnPlayerDisconnected;
            MultiplayerManager.Instance.ConnectionFailed -= OnConnectionFailed;
        }
    }

    private void BuildUI()
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        var vbox = new VBoxContainer();
        margin.AddChild(vbox);

        // Status label at top
        _statusLabel = new Label();
        _statusLabel.Text = "Not connected";
        vbox.AddChild(_statusLabel);

        var separator1 = new HSeparator();
        vbox.AddChild(separator1);

        // Tab container for Host/Join
        _tabContainer = new TabContainer();
        vbox.AddChild(_tabContainer);
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        // Host tab
        var hostTab = new VBoxContainer();
        hostTab.Name = "Host Server";
        _tabContainer.AddChild(hostTab);

        var portLabel = new Label();
        portLabel.Text = "Port:";
        hostTab.AddChild(portLabel);

        _portInput = new LineEdit();
        _portInput.Text = DefaultPort.ToString();
        _portInput.PlaceholderText = "7777";
        hostTab.AddChild(_portInput);

        var maxPlayersLabel = new Label();
        maxPlayersLabel.Text = "Max Players:";
        hostTab.AddChild(maxPlayersLabel);

        _maxPlayersInput = new SpinBox();
        _maxPlayersInput.MinValue = 2;
        _maxPlayersInput.MaxValue = 32;
        _maxPlayersInput.Value = DefaultMaxPlayers;
        hostTab.AddChild(_maxPlayersInput);

        _hostButton = new Button();
        _hostButton.Text = "Host Server";
        _hostButton.Pressed += OnHostPressed;
        hostTab.AddChild(_hostButton);

        // Join tab
        var joinTab = new VBoxContainer();
        joinTab.Name = "Join Server";
        _tabContainer.AddChild(joinTab);

        var addressLabel = new Label();
        addressLabel.Text = "Server Address:";
        joinTab.AddChild(addressLabel);

        _addressInput = new LineEdit();
        _addressInput.PlaceholderText = "127.0.0.1 or hostname";
        _addressInput.Text = "127.0.0.1";
        joinTab.AddChild(_addressInput);

        var clientPortLabel = new Label();
        clientPortLabel.Text = "Port:";
        joinTab.AddChild(clientPortLabel);

        _clientPortInput = new LineEdit();
        _clientPortInput.Text = DefaultPort.ToString();
        _clientPortInput.PlaceholderText = "7777";
        joinTab.AddChild(_clientPortInput);

        _joinButton = new Button();
        _joinButton.Text = "Join Server";
        _joinButton.Pressed += OnJoinPressed;
        joinTab.AddChild(_joinButton);

        var separator2 = new HSeparator();
        vbox.AddChild(separator2);

        // Player list
        var playersLabel = new Label();
        playersLabel.Text = "Connected Players:";
        vbox.AddChild(playersLabel);

        _playerList = new ItemList();
        _playerList.CustomMinimumSize = new Vector2(0, 100);
        vbox.AddChild(_playerList);

        // Disconnect button at bottom
        _disconnectButton = new Button();
        _disconnectButton.Text = "Disconnect";
        _disconnectButton.Pressed += OnDisconnectPressed;
        vbox.AddChild(_disconnectButton);
    }

    private void UpdateUI()
    {
        bool isConnected = MultiplayerManager.Instance?.IsMultiplayerActive == true;
        bool isServer = MultiplayerManager.Instance?.IsServer == true;

        _hostButton.Disabled = isConnected;
        _joinButton.Disabled = isConnected;
        _portInput.Editable = !isConnected;
        _addressInput.Editable = !isConnected;
        _clientPortInput.Editable = !isConnected;
        _maxPlayersInput.Editable = !isConnected;
        _disconnectButton.Disabled = !isConnected;

        if (!isConnected)
        {
            _statusLabel.Text = "Not connected";
            _playerList.Clear();
        }
        else if (isServer)
        {
            _statusLabel.Text = $"Hosting on port {_portInput.Text}";
        }
        else
        {
            _statusLabel.Text = $"Connected to {_addressInput.Text}:{_clientPortInput.Text}";
        }

        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        _playerList.Clear();

        if (MultiplayerManager.Instance?.IsMultiplayerActive == true)
        {
            foreach (var player in MultiplayerManager.Instance.Players.Values)
            {
                var displayName = player.IsLocal ? $"{player.PlayerName} (You)" : player.PlayerName;
                _playerList.AddItem(displayName);
            }
        }
    }

    private void OnHostPressed()
    {
        if (!int.TryParse(_portInput.Text, out int port))
        {
            port = DefaultPort;
        }

        int maxPlayers = (int)_maxPlayersInput.Value;

        var error = MultiplayerManager.Instance.HostServer(port, maxPlayers);
        if (error != Error.Ok)
        {
            _statusLabel.Text = $"Failed to host server: {error}";
        }
        else
        {
            // Request project sync setup
            ProjectSynchronizer.Instance?.RequestProjectSync();
        }
    }

    private void OnJoinPressed()
    {
        string address = _addressInput.Text;
        if (string.IsNullOrWhiteSpace(address))
        {
            address = "127.0.0.1";
        }

        if (!int.TryParse(_clientPortInput.Text, out int port))
        {
            port = DefaultPort;
        }

        var error = MultiplayerManager.Instance.JoinServer(address, port);
        if (error != Error.Ok)
        {
            _statusLabel.Text = $"Failed to join server: {error}";
        }
        else
        {
            _statusLabel.Text = "Connecting...";
            
            // Request project sync after connection
            CallDeferred(nameof(RequestProjectSyncDeferred));
        }
    }

    private void RequestProjectSyncDeferred()
    {
        // Wait a frame for connection to establish
        GetTree().CreateTimer(0.5).Timeout += () =>
        {
            ProjectSynchronizer.Instance?.RequestProjectSync();
        };
    }

    private void OnDisconnectPressed()
    {
        MultiplayerManager.Instance?.Disconnect();
        UpdateUI();
    }

    private void OnServerStarted()
    {
        CallDeferred(nameof(UpdateUI));
    }

    private void OnPlayerConnected(int playerId)
    {
        GD.Print($"Player connected: {playerId}");
        CallDeferred(nameof(UpdateUI));
    }

    private void OnPlayerDisconnected(int playerId)
    {
        GD.Print($"Player disconnected: {playerId}");
        CallDeferred(nameof(UpdateUI));
    }

    private void OnConnectionFailed()
    {
        _statusLabel.Text = "Connection failed";
        CallDeferred(nameof(UpdateUI));
    }
}
