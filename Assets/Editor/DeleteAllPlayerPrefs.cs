using UnityEngine;
using System.Collections;
using UnityEditor;

public class DeleteAllPlayerPrefs : ScriptableObject {
    
    //delete The playerPrefs after a confirmation dialog
    [MenuItem ("Editor/Delete Player Prefs")]
    static void DeletePrefs()
    {
        if(EditorUtility.DisplayDialog("Delete all player preferences?",
            "Are you sure you want to delete all the prefs?", "Yes", "No"))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
