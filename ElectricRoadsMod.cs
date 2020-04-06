using Klyte.Commons.Interfaces;
using System.Reflection;

[assembly: AssemblyVersion("2.0.0.4")]
namespace Klyte.ElectricRoads
{
    public class ElectricRoadsMod : BasicIUserMod<ElectricRoadsMod, ElectricRoadsController, ERPanel>
    {
        public override string SimpleName => "Electric Roads Mod";

        public override string Description => "This mod turns all roads electric conductive.";

        public override string IconName => "K45_ER_Icon";
    }
}
