using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEditor.Events;

public class ToolManager : Editor
{
    [SerializeField]
    private static ObjectSaveID[] objectsToSave;
    private static string path;

    [MenuItem("Tools/Rebuild Items Save ID")]
    static void RebuildItems()
    {
        objectsToSave = FindObjectsOfType<ObjectSaveID>();

        for (int i = 0; i < objectsToSave.Length; i++)
        {
            objectsToSave[i].objectID = i;
            EditorUtility.SetDirty(objectsToSave[i]);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("<color=green>Rebuild completed!: </color>All saviable objects have individual id");
    }


    [MenuItem("HHS/Delete Game Saves")]
    static void DeleteSaves()
    {
        path = Application.persistentDataPath + "/save.xml";
        if (File.Exists(path))
        {
            File.Delete(path);
            PlayerPrefs.SetInt("LoadGame", 0);
            PlayerPrefs.SetInt("HasSaveGame",0);
            Debug.Log("<color=green>Game Saves deleted!</color>");
        }else
        {
            Debug.Log("<color=red>Game saves not found!</color>");
        }
       
    }


    [MenuItem("HHS/BuildScene")]
    static void BuildScene()
    {
        GameObject psp = PrefabUtility.InstantiatePrefab(Resources.Load("PlayerSpawnPoint")) as GameObject;    
        GameObject esp = PrefabUtility.InstantiatePrefab(Resources.Load("EnemySpawnPoint")) as GameObject;   
        GameObject player = PrefabUtility.InstantiatePrefab(Resources.Load("Player")) as GameObject;     
        GameObject GameManager = PrefabUtility.InstantiatePrefab(Resources.Load("GameManager")) as GameObject;       
        GameObject enemy = PrefabUtility.InstantiatePrefab(Resources.Load("Enemy")) as GameObject;        
        GameObject enemyWP = PrefabUtility.InstantiatePrefab(Resources.Load("EnemyWayPoints")) as GameObject;
        GameObject winTrigger = PrefabUtility.InstantiatePrefab(Resources.Load("WinTrigger")) as GameObject;     
        psp.transform.position = new Vector3(0, 1, 0);
        winTrigger.transform.position = new Vector3(-15, 1.5f, -15);
        esp.transform.position = new Vector3(10, 0, 10);


        ////SETUP GAME CONTROLLER
        GameManager.GetComponent<GameManager>().player = player.GetComponent<FirstPersonController>();
        GameManager.GetComponent<GameManager>().inventory = player.GetComponent<Inventory>();
        GameManager.GetComponent<GameManager>().enemy = enemy.GetComponent<Enemy>();
        GameManager.GetComponent<GameManager>().playerSpawnPoint = psp.transform;
        GameManager.GetComponent<GameManager>().enemySpawnPoint = esp.transform;
      

        ////PLAYER SETUP
        player.GetComponent<FirstPersonController>().inventory = player.GetComponent<Inventory>();
        player.GetComponent<FirstPersonController>().GameManager = GameManager.GetComponent<GameManager>();
        player.GetComponent<Inventory>().database = GameManager.GetComponent<ItemsDatabase>();
        player.GetComponent<Inventory>().DropButton = GameManager.GetComponent<GameManager>().dropImage;
        player.GetComponent<FirstPersonController>().imageStand = GameManager.GetComponent<GameManager>().standImage;
        player.GetComponent<FirstPersonController>().imageCrouch = GameManager.GetComponent<GameManager>().crouchImage;
        player.GetComponent<FirstPersonController>().imageExitHidePlace = GameManager.GetComponent<GameManager>().hidePlaceExitImage;
        //player.GetComponent<FirstPersonController>().cameraTransform.GetComponent<CamInteraction>().interactButton = GameManager.GetComponent<GameManager>().interactImage;
        player.transform.position = psp.transform.position;


        ////ENEMY SETUP
        enemy.GetComponent<Enemy>().player = player.GetComponent<FirstPersonController>();
        enemy.GetComponent<Enemy>().wayPoints = new Transform[enemyWP.transform.childCount];
        enemy.transform.position = esp.transform.position;
 

        ////WINPOINT SETUP
        UnityEventTools.AddPersistentListener(winTrigger.GetComponent<TriggerEvents>().interactEvent, GameManager.GetComponent<GameManager>().GameWin);

        for (int i = 0; i < enemyWP.transform.childCount; i++)
        {
            enemy.GetComponent<Enemy>().wayPoints[i] = enemyWP.transform.GetChild(i).transform;
        }

        Debug.Log("<color=green>Your scene is ready!</color>");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }



}