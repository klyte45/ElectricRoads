using Klyte.Commons.Interfaces;

namespace Klyte.ElectricRoads.Data
{

    public class ClassesData : ExtensionInterfaceDictionaryStructValSimplImpl<ClassesData, string, bool>
    {
        public bool GetConductibility(ItemClass clazz)
        {
            bool? val = SafeGet(clazz.name);
            bool conducts = val ?? GetDefaultValueFor(clazz);
            if (val == null)
            {
                SafeSet(clazz.name, conducts);
            }

            return conducts;
        }

        public void SetConductibility(ItemClass clazz, bool value) => SafeSet(clazz.name, value);

        private static bool GetDefaultValueFor(ItemClass m_class)
        {
            return m_class.m_service == ItemClass.Service.Electricity
                || m_class.m_service == ItemClass.Service.Road
                || m_class.m_service == ItemClass.Service.Beautification
                || (m_class.m_service == ItemClass.Service.PublicTransport
                    && (m_class.m_subService == ItemClass.SubService.PublicTransportTrain
                        || m_class.m_subService == ItemClass.SubService.PublicTransportTram
                        || m_class.m_subService == ItemClass.SubService.PublicTransportMonorail
                        || m_class.m_subService == ItemClass.SubService.PublicTransportMetro
                        || m_class.m_subService == ItemClass.SubService.PublicTransportPlane)
                    && (m_class.m_layer == ItemClass.Layer.Default || m_class.m_layer == ItemClass.Layer.MetroTunnels));
        }
        public override string SaveId => "K45_ER_ClassesData";
    }

}
