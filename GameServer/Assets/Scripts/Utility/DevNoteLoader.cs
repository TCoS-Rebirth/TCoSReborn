using Database.Dynamic;
using UnityEngine;

namespace Utility
{
    public class DevNoteLoader : MonoBehaviour
    {
        [ContextMenu("Load Notes")]
        public void LoadNotes()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject, false);
            }
            foreach (var note in DB.DevNoteDB.LoadAllNotes())
            {
                var go = new GameObject("note");
                go.transform.parent = transform;
                go.transform.position = note.position;
                var dwn = go.AddComponent<DevWorldNote>();
                dwn.note = note.note;
                dwn.characterID = note.CharacterID;
                dwn.zoneID = note.mapID;
            }
        }

        public class DevNote
        {
            public int CharacterID;
            public int mapID;
            public string note;
            public Vector3 position;
        }
    }
}