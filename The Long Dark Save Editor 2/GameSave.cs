using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using The_Long_Dark_Save_Editor_2.Game_data;
using The_Long_Dark_Save_Editor_2.Helpers;
using The_Long_Dark_Save_Editor_2.Serialization;

namespace The_Long_Dark_Save_Editor_2
{

    public class CantainerRootobject
    {
        public float[] m_Position { get; set; }
        public string m_PrefabName { get; set; }
        public string m_SearializedContainer { get; set; }
        public string m_Guid { get; set; }
        public bool m_NotActive { get; set; }
    }


    public class GameSave
    {
        public static int MAX_BACKUPS = 20;

        public long LastSaved { get; set; }
        private DynamicSerializable<BootSaveGameFormat> dynamicBoot;
        public BootSaveGameFormat Boot { get { return dynamicBoot.Obj; } }

        private DynamicSerializable<GlobalSaveGameFormat> dynamicGlobal;
        public GlobalSaveGameFormat Global { get { return dynamicGlobal.Obj; } }

        public Dictionary<string, RegionData> MainRegions { get; set; } = new Dictionary<string, RegionData>();
        public Dictionary<string, RegionData> OtherRegions { get; set; } = new Dictionary<string, RegionData>();

        public AfflictionsContainer Afflictions { get; set; }

        private DynamicSerializable<SlotData> dynamicSlotData;
        public SlotData SlotData { get { return dynamicSlotData.Obj; } }

        public string OriginalRegion { get; set; }

        public string path;

        public void LoadSave(string path)
        {
            this.path = path;
            string slotJson = EncryptString.Decompress(File.ReadAllBytes(path));
            dynamicSlotData = new DynamicSerializable<SlotData>(slotJson);

            var bootJson = EncryptString.Decompress(Convert.FromBase64String(SlotData.m_Dict["boot"]));
            dynamicBoot = new DynamicSerializable<BootSaveGameFormat>(bootJson);
            OriginalRegion = Boot.m_SceneName.Value;

            var globalJson = EncryptString.Decompress(Convert.FromBase64String(SlotData.m_Dict["global"]));
            dynamicGlobal = new DynamicSerializable<GlobalSaveGameFormat>(globalJson);

            foreach (var item in SlotData.m_Dict)
            {
                if (item.Key == "global" || item.Key == "boot" || item.Key == "info" || item.Key == "screenshot")
                    continue;

                MainRegions[item.Key] = new DynamicSerializable<RegionData>(EncryptString.Decompress(Convert.FromBase64String(item.Value))).Obj;
                
                var text = EncryptString.Decompress(Convert.FromBase64String(item.Value));
                var data = DynamicSerializable<object>.GetObject(text);
            }

            Afflictions = new AfflictionsContainer(Global);
        }

        public void Save()
        {
            Backup();

            LastSaved = DateTime.Now.Ticks;
            var bootSerialized = dynamicBoot.Serialize();
            //SlotData.m_Dict["boot"] = EncryptString.Compress(bootSerialized);

            if (Boot.m_SceneName.Value != OriginalRegion)
            {
                Global.GameManagerData.SceneTransition.m_ForceNextSceneLoadTriggerScene = null;
            }
            Global.GameManagerData.SceneTransition.m_SceneSaveFilenameCurrent = Boot.m_SceneName.Value;
            Global.GameManagerData.SceneTransition.m_SceneSaveFilenameNextLoad = Boot.m_SceneName.Value;
            Global.PlayerManager.m_CheatsUsed = true;
            Afflictions.SerializeTo(Global);

            var globalSerialized = dynamicGlobal.Serialize();
            //SlotData.m_Dict["global"] = EncryptString.Compress(globalSerialized);

            SlotData.m_Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var slotDataSerialized = dynamicSlotData.Serialize();
            File.WriteAllBytes(this.path, EncryptString.Compress(slotDataSerialized));
        }

        private void Backup()
        {
            var backupDirectory = Path.Combine(Path.GetDirectoryName(this.path), "backups");
            Directory.CreateDirectory(backupDirectory);

            var oldBackups = new DirectoryInfo(backupDirectory).GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(MAX_BACKUPS);
            foreach (var file in oldBackups)
            {
                File.Delete(file.FullName);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
            var i = 1;
            var backupPath = Path.Combine(backupDirectory, timestamp + "-" + Path.GetFileName(this.path) + ".backup");
            while (File.Exists(backupPath))
            {
                backupPath = Path.Combine(backupDirectory, timestamp + "-" + Path.GetFileName(this.path) + "(" + i++ + ")" + ".backup");
            }
            File.Copy(this.path, backupPath);
        }
    }
}
