using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.ElectricRoads.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.ElectricRoads
{

    public class ERPanel : UICustomControl
    {
        private UIPanel m_controlContainer;

        public static ERPanel Instance { get; private set; }
        public UIPanel MainPanel { get; private set; }

        private Dictionary<ItemClass, List<NetInfo>> m_allClasses;

        #region Awake
        public void Awake()
        {
            Instance = this;
            m_controlContainer = GetComponent<UIPanel>();
            m_controlContainer.area = new Vector4(0, 0, 0, 0);
            m_controlContainer.isVisible = false;
            m_controlContainer.name = "ERanel";

            KlyteMonoUtils.CreateUIElement(out UIPanel _mainPanel, GetComponent<UIPanel>().transform, "ERListPanel", new Vector4(0, 0, 400, m_controlContainer.parent.height));
            MainPanel = _mainPanel;
            MainPanel.backgroundSprite = "MenuPanel2";

            CreateTitleBar();

            KlyteMonoUtils.CreateUIElement(out UIButton exportAsDefault, _mainPanel.transform, "ExportAsDefault", new Vector4(10, 50, 180, 30));
            KlyteMonoUtils.InitButtonFull(exportAsDefault, false, "ButtonMenu");
            exportAsDefault.localeID = "K45_ER_EXPORT_DEFAULT_BTN";
            exportAsDefault.minimumSize = new Vector2(180, 30);
            KlyteMonoUtils.LimitWidthAndBox(exportAsDefault);
            exportAsDefault.eventClicked += (x, y) => ClassesData.Instance.SaveAsDefault();

            KlyteMonoUtils.CreateUIElement(out UIButton importFromDefault, _mainPanel.transform, "ExportAsDefault", new Vector4(210, 50, 180, 30));
            KlyteMonoUtils.InitButtonFull(importFromDefault, false, "ButtonMenu");
            importFromDefault.localeID = "K45_ER_IMPORT_DEFAULT_BTN";
            importFromDefault.minimumSize = new Vector2(180, 30);
            KlyteMonoUtils.LimitWidthAndBox(importFromDefault, 180);
            importFromDefault.eventClicked += (x, y) => ClassesData.Instance.LoadDefaults();

            KlyteMonoUtils.CreateUIElement(out UIButton selectAll, _mainPanel.transform, "SelectAll", new Vector4(10, 90, 120, 30));
            KlyteMonoUtils.InitButtonFull(selectAll, false, "ButtonMenu");
            selectAll.localeID = "K45_ER_SELECT_ALL_BTN";
            selectAll.minimumSize = new Vector2(120, 30);
            KlyteMonoUtils.LimitWidthAndBox(selectAll, 120);
            selectAll.eventClicked += (x, y) => ClassesData.Instance.SelectAll();

            KlyteMonoUtils.CreateUIElement(out UIButton selectNone, _mainPanel.transform, "selectNone", new Vector4(140, 90, 120, 30));
            KlyteMonoUtils.InitButtonFull(selectNone, false, "ButtonMenu");
            selectNone.localeID = "K45_ER_SELECT_NONE_BTN";
            selectNone.minimumSize = new Vector2(120, 30);
            KlyteMonoUtils.LimitWidthAndBox(selectNone, 120);
            selectNone.eventClicked += (x, y) => ClassesData.Instance.UnselectAll();

            KlyteMonoUtils.CreateUIElement(out UIButton reset, _mainPanel.transform, "reset", new Vector4(270, 90, 120, 30));
            KlyteMonoUtils.InitButtonFull(reset, false, "ButtonMenu");
            reset.localeID = "K45_ER_RESET_BTN";
            reset.minimumSize = new Vector2(120, 30);
            KlyteMonoUtils.LimitWidthAndBox(reset, 120);
            reset.eventClicked += (x, y) => ClassesData.Instance.SafeCleanAll(m_allClasses.Keys);

            KlyteMonoUtils.CreateScrollPanel(_mainPanel, out UIScrollablePanel scrollPanel, out _, _mainPanel.width - 25, _mainPanel.height - 135, new Vector3(5, 130));
            scrollPanel.autoLayout = true;
            scrollPanel.autoLayoutDirection = LayoutDirection.Vertical;
            scrollPanel.autoLayoutPadding = new RectOffset(0, 0, 5, 5);
            scrollPanel.backgroundSprite = "ScrollbarTrack";
            scrollPanel.scrollPadding = new RectOffset(5, 5, 5, 5);

            m_allClasses = ((FastList<PrefabCollection<NetInfo>.PrefabData>)typeof(PrefabCollection<NetInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
                .m_buffer
                .Select(x => x.m_prefab)
                .Where(x => x?.m_class != null && (x.m_class.m_layer == ItemClass.Layer.Default || x.m_class.m_layer == ItemClass.Layer.MetroTunnels || x.m_class.m_layer == ItemClass.Layer.WaterPipes || x.m_class.m_layer == ItemClass.Layer.WaterStructures))
                .GroupBy(x => x.m_class.name)
                .ToDictionary(x => x.First().m_class, x => x.ToList());

            foreach (KeyValuePair<ItemClass, List<NetInfo>> clazz in m_allClasses)
            {
                List<NetInfo> itemList = clazz.Value;
                ItemClass clazzKey = clazz.Key;
                string className = clazzKey.name;

                KlyteMonoUtils.CreateUIElement(out UIPanel row, scrollPanel.transform, $"{clazz.Key.name}", new Vector4(0, 0, scrollPanel.width, 20));
                row.autoLayout = true;
                row.padding = new RectOffset(5, 5, 0, 0);
                row.stringUserData = className;

                UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(row, $"{clazz.Key.name}", ClassesData.Instance.GetConductibility(clazzKey));
                uiCheckbox.name = "ClassCheckbox";
                uiCheckbox.height = 20f;
                uiCheckbox.width = 335f;
                uiCheckbox.label.processMarkup = true;
                uiCheckbox.label.textScale = 0.8f;
                uiCheckbox.objectUserData = clazzKey;
                uiCheckbox.eventCheckChanged += SetItemClassValue;
                KlyteMonoUtils.LimitWidthAndBox(uiCheckbox.label, 325);
                ClassesData.Instance.eventOnValueChanged += (x, y) =>
                {
                    if (x == className && y is bool b)
                    {
                        SetNewValue(uiCheckbox, b);
                    }
                };
                ClassesData.Instance.eventAllChanged += () => SetNewValue(uiCheckbox, ClassesData.Instance.GetConductibility(clazzKey));

                KlyteMonoUtils.CreateUIElement(out UIButton help, row.transform, "?", new Vector4(0, 0, 20, 20));
                help.text = "?";
                help.hoveredTextColor = Color.blue;
                KlyteMonoUtils.InitButtonFull(help, false, "OptionBase");

                help.eventClicked += (x, y) =>
                {
                    K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                    {
                        icon = ElectricRoadsMod.Instance.IconName,
                        messageAlign = UIHorizontalAlignment.Left,
                        showButton1 = true,
                        showButton2 = true,
                        showButton3 = true,
                        showClose = true,
                        textButton1 = Locale.Get("K45_ER_ACTIVATE_CLASS_BTN"),
                        textButton2 = Locale.Get("K45_ER_DEACTIVATE_CLASS_BTN"),
                        textButton3 = Locale.Get("K45_ER_RETURN_BTN"),
                        title = string.Format(Locale.Get("K45_ER_TITLE_NET_LIST_WINDOW"), className),
                        message = string.Format(Locale.Get(itemList.Count <= 20 ? "K45_ER_PATTERN_NET_LIST_FEW" : "K45_ER_PATTERN_NET_LIST_FULL"), string.Join("\n", itemList.Take(20).Select(x => $"\t- {x.GetLocalizedTitle()}").ToArray()), itemList.Count - 20)
                    }, (x) =>
                    {
                        if (x == 1)
                        {
                            uiCheckbox.isChecked = true;
                        }
                        else if (x == 2)
                        {
                            uiCheckbox.isChecked = false;
                        }

                    });
                };
            }
            Quicksort(scrollPanel.components, new Comparison<UIComponent>(CompareNames), false);
        }

        private void SetNewValue(UICheckBox uiCheckbox, bool b)
        {
            uiCheckbox.eventCheckChanged -= SetItemClassValue;
            uiCheckbox.isChecked = b;
            uiCheckbox.eventCheckChanged += SetItemClassValue;
        }

        private void SetItemClassValue(UIComponent component, bool value) => ClassesData.Instance.SetConductibility((ItemClass)component.objectUserData, value);
        private static int CompareNames(UIComponent left, UIComponent right) => string.Compare(left.stringUserData, right.stringUserData, StringComparison.InvariantCulture);
        protected static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp, bool invert) => SortingUtils.Quicksort(elements, comp, invert);
        private void CreateTitleBar()
        {
            KlyteMonoUtils.CreateUIElement(out UILabel titlebar, MainPanel.transform, "TLMListPanel", new Vector4(0, 0, MainPanel.width - 150, 20));
            titlebar.position = default;
            titlebar.autoSize = false;
            titlebar.text = $"{ElectricRoadsMod.Instance.SimpleName} v{ElectricRoadsMod.Version}";
            titlebar.textAlignment = UIHorizontalAlignment.Center;
            titlebar.relativePosition = new Vector3(75, 13);

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

        //        foreach (IGrouping<string, NetInfo> clazz in classes)
        //            {
        //                if (!ClassesData.Instance.SafeGet(clazz.Key) == null)
        //                {
        //                    var itemList = clazz.ToList();
        //        ElectricRoadsOverrides.conductsElectricity[clazz.Key] = ElectricRoadsOverrides.getDefaultValueFor(itemList[0].m_class);
        //                    var checkbox = (UICheckBox)gr.AddCheckbox(clazz.Key, ElectricRoadsOverrides.conductsElectricity[clazz.Key], (x) => ElectricRoadsOverrides.conductsElectricity[clazz.Key] = x);
        //        checkbox.tooltip = $"Affected assets:\n{string.Join("\n", itemList?.Select(x => Locale.Exists("NET_TITLE", x?.name) ? Locale.Get("NET_TITLE", x?.name) : x.m_isCustomContent ? null : x?.name)?.Where(x => x != null)?.ToArray())}";
        //                }
        //}

        #endregion
    }
}
