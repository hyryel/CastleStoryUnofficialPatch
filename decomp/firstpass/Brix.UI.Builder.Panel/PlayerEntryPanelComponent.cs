using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class PlayerEntryPanelComponent : BaseSocketLayoutPanelComponent
{
	public Text _playerNameLabel;

	public GameObject _hostGo;

	public LayoutElement _hostLayoutElement;

	public GameObject _readyGo;

	public Image _readyImage;

	public Button _readyButton;

	public GameObject _kickGo;

	public Button _kickButton;

	public GameObject _pingGo;

	public Text _pingKeyLabel;

	public Text _pingValueLabel;

	public GameObject _downloadingGo;

	public Text _downloadingKeyLabel;

	public Text _downloadingValueLabel;

	public RectTransform _downloadingProgressTrs;

	public GameObject _factionGo;

	public Dropdown _factionDropdown;

	public EventTrigger _factionEventTrigger;

	public RectTransform _factionArrowTrs;

	public Text _factionKeyLabel;

	public Text _factionValueLabel;

	public Image _factionValueImage;

	public GameObject _teamGo;

	public Dropdown _teamDropdown;

	public EventTrigger _teamEventTrigger;

	public RectTransform _teamArrowTrs;

	public Text _teamKeyLabel;

	public Text _teamValueLabel;

	public Image _teamValueImage;

	public RectTransform _playerBin;
}
