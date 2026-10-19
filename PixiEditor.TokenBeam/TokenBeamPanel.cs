using System;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.TokenBeam;

public class TokenBeamPanel : StatefulElement<TokenBeamPanelState>
{
    public event Action<TokenBeamSession> OnSessionEstablished;

    private TokenBeamSession? currentSession;

    public TokenBeamPanel(TokenBeamSession? currentSession)
    {
        this.currentSession = currentSession;
    }

    public override TokenBeamPanelState CreateState()
    {
        var state = new TokenBeamPanelState(currentSession);
        state.OnSessionEstablished += OnSessionEstablished;
        return state;
    }
}

public class TokenBeamPanelState : State
{
    private TextField TokenField => tokenField;
    private TokenBeamSession? Session => session;
    private TextField tokenField;
    private TextField serverField;
    private TokenBeamSession? session;
    public event Action<TokenBeamSession> OnSessionEstablished;

    public TokenBeamPanelState(TokenBeamSession? beamSession)
    {
        session = beamSession;

        if (session != null)
        {
            session.OnConnected += OnConnected;
            session.OnConnectionLost += OnConnectionLost;
        }
    }

    public override LayoutElement BuildElement()
    {
        tokenField = new TextField(
            placeholder: "Token");

        serverField = new TextField(
            placeholder: "Server address", text: "wss://tokenbeam.dev");

        if (session is { Connected: true } && !string.IsNullOrEmpty(session.BeamToken))
        {
            tokenField.Text = session.BeamToken;
        }

        bool connected = session?.Connected == true;

        Color statusColor = connected
            ? Color.FromRgba(25, 110, 65, 255)
            : Color.FromRgba(55, 55, 55, 255);

        return new Container(
            margin: Edges.All(24),
            child: new Column(
                crossAxisAlignment: CrossAxisAlignment.Stretch,
                children:
                [
                    new Text(
                        "Token Beam",
                        textStyle: new TextStyle(
                            fontSize: 24,
                            fontWeight: FontWeight.Bold)
                    ),

                    new Container(
                        margin: Edges.Symmetric(8, 0),
                        child: new Text(
                            "Connect this extension to your Token Beam workspace.",
                            wrap: TextWrap.Wrap,
                            textStyle: new TextStyle(fontSize: 14)
                        )
                    ),

                    new Container(
                        margin: Edges.Symmetric(16, 0),
                        child: new Container(
                            backgroundColor: statusColor,
                            margin: Edges.All(0),
                            child: new Container(
                                margin: Edges.All(16),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.Stretch,
                                    children:
                                    [
                                        new Text(
                                            "Connection status",
                                            textStyle: new TextStyle(
                                                fontSize: 12)
                                        ),

                                        new Container(
                                            margin: Edges.Symmetric(4, 0),
                                            child: new Text(
                                                connected ? "Connected" : "Not connected",
                                                textStyle: new TextStyle(
                                                    fontSize: 16,
                                                    fontWeight: FontWeight.Bold)
                                            )
                                        )
                                    ]
                                )
                            )
                        )
                    ),
                    new Container(
                        margin: Edges.Symmetric(4, 0),
                        child: new Text(
                            "Access token",
                            textStyle: new TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.Bold)
                        )
                    ),

                    new Row(
                        mainAxisAlignment: MainAxisAlignment.SpaceEvenly,
                        children:
                        [
                            new Container(
                                margin: Edges.Symmetric(8, 0),
                                width: 200,
                                child: serverField
                            ),
                            new Container(
                                margin: Edges.Symmetric(8, 0),
                                width: 150,
                                child: tokenField
                            )
                        ]
                    ),
                    new Container(
                        margin: Edges.Symmetric(4, 0),
                        child: new Text(
                            "Enter the access token for your Token Beam workspace.",
                            wrap: TextWrap.Wrap,
                            textStyle: new TextStyle(fontSize: 12)
                        )
                    ),
                    new Container(
                        margin: Edges.Symmetric(24, 0),
                        child: new Button(
                            child: new Text(
                                connected ? "Disconnect" : "Connect"),
                            onClick: _ =>
                            {
                                if (Session?.Connected == true)
                                {
                                    Disconnect();
                                    return;
                                }

                                Connect();
                            }
                        ))
                ]
            )
        );
    }


    private void Connect()
    {
        if (string.IsNullOrWhiteSpace(TokenField.Text))
            return;

        var newSession = new TokenBeamSession(TokenField.Text, serverField.Text);

        if (Session != null)
        {
            Session.OnConnected -= OnConnected;
            Session.OnConnectionLost -= OnConnectionLost;
            Session.Disconnect();
        }

        session = newSession;

        session.OnConnected += OnConnected;
        session.OnConnectionLost += OnConnectionLost;

        OnSessionEstablished?.Invoke(session);

        SetState(() => { });
    }

    private void Disconnect()
    {
        if (session == null)
            return;

        var oldSession = session;

        oldSession.OnConnected -= OnConnected;
        oldSession.OnConnectionLost -= OnConnectionLost;

        session = null;

        oldSession.Disconnect();

        SetState(() => { });
    }

    private void OnConnected()
    {
        SetState(() => { });
    }

    private void OnConnectionLost()
    {
        SetState(() => { });
    }
}