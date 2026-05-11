  use pixieditor_sdk::*;

  #[unsafe(no_mangle)]
  pub extern "C" fn initialize() {
      let spray = CustomToolConfig {
          name: "SPRAY".to_string(),
          icon: "/Icons/spray-can.svg".to_string(),
          tool_tip: "SPRAY_TOOLTIP".to_string(),
          default_shortcut: Some(Shortcut { key: 0, modifiers: 0 }),
          supports_secondary_action_on_right_click: false,
          actions_display_configs: vec![
              ActionDisplayConfig {
                  text: "SPRAY_ACTION_DISPLAY".to_string(),
                  modifiers: 0
              }
          ],
          common_tool_type: "".to_string()
      };

      let mut buf_spray = Vec::new();
      spray.encode(&mut buf_spray).unwrap();

      register_tool(
          load_resource("/Spray.pixi").as_slice(),
          &buf_spray,
      );

      let tool_config = "{
          \"DefaultToolSize\": 10
      }";

      add_to_toolset_with_config("SPRAY", "PixiEditor:PAINT_TOOLSET", 8, tool_config);
      add_to_toolset_with_config("SPRAY", "PixiEditor:PIXEL_ART_TOOLSET", 8, tool_config);
  }