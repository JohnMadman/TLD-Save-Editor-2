using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using The_Long_Dark_Save_Editor_2.Serialization;

namespace The_Long_Dark_Save_Editor_2.Game_data
{
    public class RegionData
    {
        public int m_Version { get; set; }

        [Deserialize("m_GearManagerSerialized", true)]
        public GearManagerData GearManagerData { get; set; }
    }

    public class GearManagerData
    {
        [Deserialize("m_SerializedItems")]
        public ObservableCollection<InventoryItemSaveData> Items { get; set; }
    }
}
