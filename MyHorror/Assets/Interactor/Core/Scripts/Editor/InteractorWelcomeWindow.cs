using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;

namespace razz
{
    public class InteractorWelcomeWindow : EditorWindow
    {
        private static InteractorWelcomeWindow _instance;
        private bool _init;

        #region File Variables
        private readonly string repoUrl = "https://github.com/razzraziel/InteractorExamples/archive/refs/tags/";
        private readonly string repoName = "InteractorExamples-";
        private readonly string examplespackage = "Examples";
        private readonly string textures1package = "Textures1";
        private readonly string textures2package = "Textures2";
        private readonly string fileformat = ".zip";

        private readonly float examplessize = 40.1f;
        private readonly float textures1size = 97f;
        private readonly float textures2size = 64.4f;

        private bool examplespackageExist = true;
        private bool examplespackageImported = false;
        private string downloadUrl;

        private readonly string localPath = "Temp/InteractorDownload";
        private readonly string examplespackagePath = "Assets/Interactor/Integrations/Examples.unitypackage";

        private bool isDownloading = false;
        private string statusMessage = "";
        private CancellationTokenSource cancellationTokenSource;
        private float downloadProgress = 0f;
        private float currentFileSize;
        private string currentFileName;
        private ulong downloadedBytes = 0;
        #endregion

        #region UI Variables
        private GUISkin _skin;
        private GUIStyle _background;
        private static Vector2 _size = new Vector2(600, 480);
        private string[] items = { "Update Note", "Upcoming v1.0", "Import Examples", "Tutorials", "Support" };
        private int leftPanelSize = 200;
        private int selectedItemIndex = 0;
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;
        private Color leftBackgroundColor;
        private Color rightBackgroundColor;
        private GUIStyle customButtonStyle;
        private GUIStyle readOnlyTextAreaStyle;
        private GUIStyle centeredLabelStyle;
        private Texture2D[] buttonIcons;
        private Texture2D headerImage;
        private string htmlColor = "DD3030";
        #endregion

        #region File Handling
        public async void PrepareDownload(int file)
        {
            isDownloading = true;

            switch (file)
            {
                case 0:
                    {
                        currentFileName = examplespackage;
                        currentFileSize = examplessize;
                        downloadUrl = repoUrl + examplespackage + fileformat; break;
                    }
                case 1:
                    {
                        currentFileName = textures1package;
                        currentFileSize = textures1size;
                        downloadUrl = repoUrl + textures1package + fileformat; break;
                    }
                case 2:
                    {
                        currentFileName = textures2package;
                        currentFileSize = textures2size;
                        downloadUrl = repoUrl + textures2package + fileformat; break;
                    }
            }

            bool exists = true;
#if UNITY_2022_1_OR_NEWER
            exists = await CheckUrlExists(downloadUrl);
#endif
            if (exists) StartDownload(); //Direct Github links (repoUrl)
            else
            { //Hotlink from online documentation in case if Github links are gg
                switch (file)
                {
                    case 0: downloadUrl = Links.downloadExamplesURL; break;
                    case 1: downloadUrl = Links.downloadTextures1URL; break;
                    case 2: downloadUrl = Links.downloadTextures2URL; break;
                }
                cancellationTokenSource = new CancellationTokenSource();
                downloadUrl = await GetRedirectedUrlAsync(downloadUrl, cancellationTokenSource.Token); //Gets HTML and parses new urls
                StartDownload();
            }
        }
        public async Task<bool> CheckUrlExists(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Head(url))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

#if UNITY_2020_1_OR_NEWER
                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Error checking URL: {webRequest.error}");
                    Debug.Log("Getting alternative link...");
                    return false;
                }
#else
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.Log($"Error checking URL: {webRequest.error}");
                    Debug.Log("Getting alternative link...");
                    return false;
                }
#endif

                bool exists = (webRequest.responseCode >= 200 && webRequest.responseCode < 300);

                return exists;
            }
        }
        private async Task<string> GetRedirectedUrlAsync(string initialUrl, CancellationToken cancellationToken)
        {
            using (var www = new UnityWebRequest(initialUrl, UnityWebRequest.kHttpVerbGET))
            {
                www.downloadHandler = new DownloadHandlerBuffer();

                var operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        www.Abort();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    await Task.Delay(100);
                }

#if UNITY_2020_3_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    cancellationTokenSource?.Cancel();
                    isDownloading = false;
                    throw new Exception($"Failed to fetch redirected URL: {www.error}");
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    cancellationTokenSource?.Cancel();
                    isDownloading = false;
                    throw new Exception($"Failed to fetch redirected URL: {www.error}");
                }
#endif

                string responseText = www.downloadHandler.text;
                string metaTagPrefix = "<meta http-equiv=\"refresh\" content=\"0; url=";
                int startIndex = responseText.IndexOf(metaTagPrefix, StringComparison.OrdinalIgnoreCase);
                if (startIndex != -1)
                {
                    startIndex += metaTagPrefix.Length;
                    int endIndex = responseText.IndexOf("\"", startIndex);
                    if (endIndex != -1)
                    {
                        return responseText.Substring(startIndex, endIndex - startIndex);
                    }
                }

                throw new Exception("No valid redirection URL found in the response.");
            }
        }
        private void StartDownload()
        {
            downloadProgress = 0f;
            downloadedBytes = 0;
            statusMessage = "Starting download: " + downloadUrl;
            cancellationTokenSource = new CancellationTokenSource();

            ExecuteDownloadAsync(cancellationTokenSource.Token)
                .ContinueWith(task =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        HandleDownloadCompletion(task);
                    };
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private string GetDownloadStatus()
        {
            string downloadedMB = (downloadedBytes / 1024f / 1024f).ToString("F1");
            string totalMB = currentFileSize.ToString("F1");

            return $"Downloading: {downloadedMB} MB / {totalMB} MB";
        }
        private string FormatBytes(ulong bytes)
        {
            return (bytes / 1024f / 1024f).ToString("F1") + " MB";
        }
        private string GetFileNameFromUrl(string url)
        {
            int lastSlash = url.LastIndexOf('/');
            return lastSlash >= 0 ? url.Substring(lastSlash + 1) : "download.unitypackage";
        }
        private string GetTemporaryDownloadPath(string fileName)
        {
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            return localPath + "/" + fileName;
        }
        private async Task ExecuteDownloadAsync(CancellationToken cancellationToken)
        {
            Debug.Log(statusMessage);
            string fileName = GetFileNameFromUrl(downloadUrl);
            string tempPath = GetTemporaryDownloadPath(fileName);

            using (var www = new UnityWebRequest(downloadUrl, UnityWebRequest.kHttpVerbGET))
            {
                www.downloadHandler = new DownloadHandlerFile(tempPath);

                try
                {
                    var operation = www.SendWebRequest();
                    while (!operation.isDone)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            www.Abort();
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        downloadedBytes = www.downloadedBytes;
                        //downloadProgress = www.downloadProgress;
                        downloadProgress = ((float)downloadedBytes / currentFileSize) / 1048576;
                        await Task.Delay(100);
                    }

#if UNITY_2020_3_OR_NEWER
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        throw new Exception(www.error);
                    }
#else
                    if (www.isNetworkError || www.isHttpError)
                    {
                        throw new Exception(www.error);
                    }
#endif
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Download operation was canceled.");
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Download failed: {ex.Message}");
                    throw;
                }
            }
        }
        private void HandleDownloadCompletion(Task downloadTask)
        {
            try
            {
                if (downloadTask.IsCanceled)
                {
                    statusMessage = "Download cancelled.";
                    CleanupDownload();
                    return;
                }

                if (downloadTask.IsFaulted)
                {
                    statusMessage = $"Error: {downloadTask.Exception?.InnerException?.Message}";
                    Debug.Log($"Download failed: {downloadTask.Exception}");
                    CleanupDownload();
                    return;
                }

                string fileName = GetFileNameFromUrl(downloadUrl);
                string tempPath = GetTemporaryDownloadPath(fileName);

                isDownloading = false;
                Repaint();
                downloadProgress = 0f;
                downloadedBytes = 0;

                if (File.Exists(tempPath))
                {
                    statusMessage = $"Download complete! Unpacking...";
                    Debug.Log(statusMessage);
                    UnzipAndCopyToAssets(tempPath, GetTemporaryDownloadPath(""));
                    statusMessage = "Unpacking complete, importing!";
                    Debug.Log(statusMessage);
                }
                else
                {
                    statusMessage = "Error: Downloaded file not found";
                    Debug.Log(statusMessage);
                }
            }
            finally
            {
                isDownloading = false;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                Repaint();
            }
        }
        public void UnzipAndCopyToAssets(string tempPath, string assetFolderPath)
        {
            if (File.Exists(tempPath))
            {
                string fileName = tempPath.Substring(tempPath.LastIndexOf('/') + 1);
                string folderName = fileName.Substring(0, fileName.LastIndexOf('.'));
                string unzipDirectory = assetFolderPath + folderName;

                if (!Directory.Exists(unzipDirectory))
                {
                    Directory.CreateDirectory(unzipDirectory);
                }
                else
                {
                    Directory.Delete(unzipDirectory, true);
                    Directory.CreateDirectory(unzipDirectory);
                }

#if UNITY_2021_3_OR_NEWER
                ZipFile.ExtractToDirectory(tempPath, unzipDirectory);
#else
                Debug.Log("Unity versions older than 2021 may not support auto unzip. Please import the file yourself after unzipping. If you close this window downloaded files will be deleted. Filepath: " + Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length) + "/" + tempPath);
#endif
                string unzippedPackFile = unzipDirectory + "/" + repoName + currentFileName + "/" + currentFileName + ".unitypackage";

                if (File.Exists(unzippedPackFile))
                {
                    AssetDatabase.ImportPackage(unzippedPackFile, true);
                    AssetDatabase.Refresh();
                }

                /*File.Delete(tempPath);
                AssetDatabase.Refresh();*/

                statusMessage = "Import complete!";
            }
        }
        private void CleanupDownload()
        {
            isDownloading = false;
            downloadProgress = 0f;
            downloadedBytes = 0;
            string fileName = GetFileNameFromUrl(downloadUrl);
            string tempPath = GetTemporaryDownloadPath(fileName);

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            Repaint();
        }
        #endregion

        #region GUI
        [MenuItem("Window/Interactor/Interactor Welcome Window", priority = 1)]
        public static void OpenWelcome()
        {
            _instance = GetWindow<InteractorWelcomeWindow>(true, "Welcome to Interactor v" + Interactor.Version, false);

            _instance.minSize = _size;
            _instance.maxSize = _size;
            _instance.Show();
        }

        private void OnEnable()
        {
            leftBackgroundColor = new Color(0.2f, 0.4f, 0.4f, 0.1f);
            rightBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            buttonIcons = new Texture2D[items.Length];
            buttonIcons[0] = EditorGUIUtility.FindTexture("d_Profiler.Audio");
            buttonIcons[1] = EditorGUIUtility.FindTexture("d_Profiler.Audio");
            buttonIcons[2] = EditorGUIUtility.FindTexture("CustomTool");
            buttonIcons[3] = EditorGUIUtility.FindTexture("d_Profiler.Video");
            buttonIcons[4] = EditorGUIUtility.FindTexture("console.infoicon.sml");


            headerImage = Resources.Load<Texture2D>("Images/TopLogoResized");

            if (File.Exists(examplespackagePath))
                examplespackageExist = true;
            else examplespackageExist = false;
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            if (Directory.Exists(localPath))
            {
                try
                {
                    Directory.Delete(localPath, true);
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Failed to cleanup Temp directory: {e.Message}");
                }
            }

            if (examplespackageImported && examplespackageExist)
            {
                try
                {
                    File.Delete(examplespackagePath);
                    if (File.Exists(examplespackagePath + ".meta"))
                        File.Delete(examplespackagePath + ".meta");
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Failed to cleanup Examples Package: {e.Message}");
                }
            }
            AssetDatabase.Refresh();
        }

        private void GetStyles()
        {
            Color textColor = Color.white;
            if (!EditorGUIUtility.isProSkin) textColor = Color.black;
            else htmlColor = "12D5DF";

            customButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(5, 5, 2, 2),
                fontSize = 12,
                fixedHeight = 30
            };

            readOnlyTextAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5),
                normal = { textColor = textColor },
                hover = { textColor = textColor },
                onHover = { textColor = textColor },
                richText = true
            };

            centeredLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = textColor },
                hover = { textColor = textColor },
                onHover = { textColor = textColor }
            };

            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _background = _skin.GetStyle("Background20Style");

            _init = true;
        }

        private void OnGUI()
        {
            if (!_init) GetStyles();
            if (!_instance) GetWindow<InteractorWelcomeWindow>(true, "Interactor v" + Interactor.Version, false);

            Rect windowRect = new Rect(0, 0, position.width, position.height + 50);
            GUI.Box(windowRect, "", _background);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(0, 0, leftPanelSize, position.height), leftBackgroundColor);
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), rightBackgroundColor);
            }

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            if (windowRect.Contains(Event.current.mousePosition) || isDownloading) this.Repaint();
        }

        private void DrawLeftPanel()
        {
            GUI.enabled = !isDownloading;
            EditorGUILayout.BeginVertical(GUILayout.Width(leftPanelSize));

            EditorGUILayout.Space();

            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos);

            for (int i = 0; i < items.Length; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = selectedItemIndex == i ? Color.gray : Color.white;

                if (GUILayout.Button(new GUIContent(items[i], buttonIcons[i]), customButtonStyle))
                {
                    selectedItemIndex = i;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos);

            switch (selectedItemIndex)
            {
                case 0:
                    DrawPage0Content();
                    break;
                case 1:
                    DrawPage1Content();
                    break;
                case 2:
                    DrawPage2Content();
                    break;
                case 3:
                    DrawPage3Content();
                    break;
                case 4:
                    DrawPage4Content();
                    break;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (headerImage != null)
            {
                GUILayout.Box(headerImage, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(85));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawPage0Content()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("What's New", centeredLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            GUILayout.Label("New Version v0.999");
            EditorGUI.indentLevel--;

            GUI.enabled = false;
            EditorGUILayout.TextArea("Interactor Full Body IK is more stable and allows you to adjust elbow bending direction on each InteractorTarget." +
                "\n\nHit Reaction for Full Body IK to create body reactions for physical impacts." +
                "\n\nHit Controller for held object animations with custom keys, events and bezier paths. Easy to use, adjust, and highly flexible." +
                "\n\nNew example scene to showcase new features." +
                "\n\nSee the full changelog for more changes and videos with details.", readOnlyTextAreaStyle, GUILayout.Height(210));
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" Full Changelog", EditorGUIUtility.FindTexture("d_BuildSettings.Web.Small")), GUILayout.Width(120)))
            {
                Links.Changelog();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DrawPage1Content()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("What's Next", centeredLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            GUILayout.Label("Upcoming Version 1.0");
            EditorGUI.indentLevel--;

            GUI.enabled = false;
            EditorGUILayout.TextArea("v1.0 won't be the final update but will make Interactor fully feature-complete." +
                "\n\nThis release will brings full API documentation and fully updated tutorials and example scenes." +
                "\n\nTouches, Multiple, TwoHands, Push interactions will be finalized with new features." +
                "\n\n\nInteractor has received ongoing updates and support since its first release and that won’t stop here. <i><color=#" + htmlColor + ">Your reviews help me deliver more updates.</color></i>", readOnlyTextAreaStyle, GUILayout.Height(220)); //give extra for 2019.4
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" Review Page", EditorGUIUtility.FindTexture("d_BuildSettings.Web.Small")), GUILayout.Width(110)))
            {
                Links.Store(true);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DrawPage2Content()
        {
            bool enableButtons = !isDownloading;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Manage Interactor Example Files", centeredLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.enabled = false;
            EditorGUILayout.TextArea("All Interactor Examples (scenes, models, scripts) will be imported (except textures). You can delete Examples folder or reimport anytime.", readOnlyTextAreaStyle, GUILayout.Height(65));
            GUI.enabled = enableButtons;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            string examplesButtonText = "Import Interactor Examples";
            string examplesButtonLogo = "d_Profiler.Open";
            if (!examplespackageExist)
            {
                examplesButtonText = " Import Interactor Examples (" + examplessize + " MB)";
                examplesButtonLogo = "SceneLoadIn";
            }

            if (GUILayout.Button(new GUIContent(examplesButtonText, EditorGUIUtility.FindTexture(examplesButtonLogo))))
            {
                if (examplespackageExist)
                {
                    examplespackageImported = true;

                    AssetDatabase.ImportPackage(examplespackagePath, true);
                }
                else if (!examplespackageExist)
                {
                    PrepareDownload(0);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUI.enabled = false;
            EditorGUILayout.TextArea("Textures are not included in the Interactor package to save space. You can download and import them anytime. Both packages are needed for all example models.", readOnlyTextAreaStyle, GUILayout.Height(65));
            GUI.enabled = enableButtons;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" Textures 1 (" + textures1size + " MB)", EditorGUIUtility.FindTexture("SceneLoadIn"))))
            {
                PrepareDownload(1);
            }

            if (GUILayout.Button(new GUIContent(" Textures 2 (" + textures2size + " MB)", EditorGUIUtility.FindTexture("SceneLoadIn"))))
            {
                PrepareDownload(2);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUI.enabled = false;
            EditorGUILayout.TextArea("Additionally, 4K resolution textures are available but must be downloaded manually due to their size (1.4 GB).", readOnlyTextAreaStyle, GUILayout.Height(50));
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Download 4K Textures via Google Drive", EditorGUIUtility.FindTexture("d_Linked"))))
            {
                Application.OpenURL(Links.texturepack4k);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

#if UNITY_2020_1_OR_NEWER
            EditorGUILayout.Space(30);
#else
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
#endif

            float downloadAreaHeight = 45f;
            Rect downloadRect = GUILayoutUtility.GetRect(0, downloadAreaHeight);

            if (isDownloading)
            {
                float progressBarHeight = 20f;
                Rect progressRect = new Rect(downloadRect.x, downloadRect.y, downloadRect.width, progressBarHeight);
                Rect cancelRect = new Rect(downloadRect.x, downloadRect.y + progressBarHeight + 5f, downloadRect.width, 20f);

                EditorGUI.ProgressBar(progressRect, downloadProgress, GetDownloadStatus());
                GUI.enabled = true;
                if (GUI.Button(cancelRect, "Cancel"))
                {
                    cancellationTokenSource?.Cancel();
                    isDownloading = false;
                }
                GUI.enabled = enableButtons;
            }

            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }

        private void DrawPage3Content()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Watch Interactor Tutorials", centeredLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.enabled = false;
            EditorGUILayout.TextArea("You can find Interactor tutorials on YouTube to help guide you through the features and functionalities. More detailed tutorials will be added, and most videos will be updated to version 1.0." +
                "\n\nIf you’re unable to find the information you need, please don’t hesitate to visit the support page.", readOnlyTextAreaStyle, GUILayout.Height(115));
            GUI.enabled = true;

            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" Interactor Tutorials Page", EditorGUIUtility.FindTexture("d_BuildSettings.Web.Small")), GUILayout.Width(180)))
            {
                Links.Tutorials();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DrawPage4Content()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Get Interactor Support", centeredLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.enabled = false;
            EditorGUILayout.TextArea("If you're having issues, encountering bugs, or have any ideas or questions, get support through these channels!", readOnlyTextAreaStyle, GUILayout.Height(50));
            GUI.enabled = true;

            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" Support Page", EditorGUIUtility.FindTexture("d_BuildSettings.Web.Small")), GUILayout.Width(120)))
            {
                Links.Support();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}
