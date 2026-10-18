namespace Brix.Utils.UI;

public class KeyBindingsList : IKeyBindingsList<KeyBinding>
{
	public static KeyBinding Construction = new KeyBinding("tb_constr_menu_toggle", "tb_constr_menu");

	public static KeyBinding Blueprints = new KeyBinding("tb_constr_blueprints_menu_toggle", "tb_constr_menu");

	KeyBinding[] IKeyBindingsList<KeyBinding>.keyData => new KeyBinding[2] { Construction, Blueprints };
}
