using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.Sdk;

namespace PixiEditor.TokenBeam;

public class TokenBeamExtension : PixiEditorExtension
{
    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        Api.Commands.RegisterCommand(new CommandMetadata("BeamSettings")
            {
                MenuItemPath = "EDIT/Token Beam Settings",
            },
            () =>
            {
                var window = Api.WindowProvider.CreatePopupWindow("Token Beam Settings", new TokenBeamPanel());
                window.Width = 400;
                window.Height = 300;
                window.Show();
            });
    }
}