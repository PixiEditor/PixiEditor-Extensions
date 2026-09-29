using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.TokenBeam;

public class TokenBeamPanel : StatefulElement<TokenBeamPanelState>
{
    public override TokenBeamPanelState CreateState()
    {
        return new TokenBeamPanelState();
    }
}

public class TokenBeamPanelState : State
{
    private TextField tokenField;
    private TokenBeamSession? session;

    public override LayoutElement BuildElement()
    {
        tokenField = new TextField(
            placeholder: "Enter your Token Beam token");

        bool connected = session?.Connected == true;

        return new Container(
            margin: Edges.All(24),
            child: new Column(
                crossAxisAlignment: CrossAxisAlignment.Stretch,
                children:
                [
                    new Text(
                        "Token Beam",
                        textStyle: new TextStyle(
                            fontSize: 22,
                            fontWeight: FontWeight.Bold)
                    ),

                    new Text(
                        "Connect this extension to your Token Beam workspace.",
                        wrap: TextWrap.Wrap,
                        textStyle: new TextStyle(
                            fontSize: 14)
                    ),

                    new Container(
                        margin: Edges.Symmetric(0, 18),
                        backgroundColor: connected
                            ? Color.FromRgba(25, 80, 45, 255)
                            : Color.FromRgba(45, 45, 45, 255),
                        child: new Row(
                            children:
                            [
                                new Text(
                                    connected ? "Connected" : "Not connected",
                                    textStyle: new TextStyle(
                                        fontWeight: FontWeight.Bold)
                                )
                            ]
                        )
                    ),

                    new Text(
                        "Access token",
                        textStyle: new TextStyle(
                            fontWeight: FontWeight.Bold)
                    ),

                    tokenField,

                    new Container(
                        margin: Edges.Symmetric(0, 16),
                        child: new Row(
                            mainAxisAlignment: MainAxisAlignment.End,
                            children:
                            [
                                new Button(
                                    child: new Text(
                                        connected ? "Disconnect" : "Connect"),
                                    onClick: _ =>
                                    {
                                        if (connected)
                                        {
                                            session?.Disconnect();
                                            return;
                                        }

                                        Connect();
                                    })
                            ]
                        )
                    )
                ]
            )
        );
    }


    private void Connect()
    {
        if (string.IsNullOrWhiteSpace(tokenField.Text))
            return;

        SetState(() =>
        {
            if (session != null)
            {
                session.Disconnect();
                session.OnConnected -= OnConnected;
                session.OnConnectionLost -= OnConnectionLost;
            }

            session = new TokenBeamSession(tokenField.Text);

            session.OnConnected += OnConnected;
            session.OnConnectionLost += OnConnectionLost;
        });
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