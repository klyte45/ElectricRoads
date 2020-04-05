using ColossalFramework.UI;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.ElectricRoads
{

    public class ERPanel : UICustomControl
    {
        private UIPanel m_controlContainer;

        public static ERPanel Instance { get; private set; }
        public UIPanel MainPanel { get; private set; }

        #region Awake
        public void Awake()
        {
            Instance = this;
            m_controlContainer = GetComponent<UIPanel>();
            m_controlContainer.area = new Vector4(0, 0, 0, 0);
            m_controlContainer.isVisible = false;
            m_controlContainer.name = "ERanel";

            KlyteMonoUtils.CreateUIElement(out UIPanel _mainPanel, GetComponent<UIPanel>().transform, "ERListPanel", new Vector4(0, 0, 875, m_controlContainer.parent.height));
            MainPanel = _mainPanel;
            MainPanel.backgroundSprite = "MenuPanel2";

            CreateTitleBar();

        }

        private void CreateTitleBar()
        {
            KlyteMonoUtils.CreateUIElement(out UILabel titlebar, MainPanel.transform, "TLMListPanel", new Vector4(75, 10, MainPanel.width - 150, 20));
            titlebar.autoSize = false;
            titlebar.text = $"{ElectricRoadsMod.Instance.SimpleName} v{ElectricRoadsMod.Version}";
            titlebar.textAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out UIButton closeButton, MainPanel.transform, "CloseButton", new Vector4(MainPanel.width - 37, 5, 32, 32));
            KlyteMonoUtils.InitButton(closeButton, false, "buttonclose", true);
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.eventClick += (x, y) =>
            {
                ElectricRoadsMod.Instance.ClosePanel();
            };

            KlyteMonoUtils.CreateUIElement(out UISprite logo, MainPanel.transform, "TLMLogo", new Vector4(22, 5f, 32, 32));
            logo.spriteName = ElectricRoadsMod.Instance.IconName;
        }

        #endregion
    }
}
