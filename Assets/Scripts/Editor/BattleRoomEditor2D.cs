using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BattleRoom))]
public class BattleRoomEditor2D : Editor
{
    private BattleRoom _battleRoom;
    private int _selectedWaveIndex = -1;
    private int _selectedSpawnIndex = -1;
    private bool _isCreatingNewSpawn = false;
    private bool _isCreatingNewWave = false;
    private Vector3 _newSpawnPosition = Vector3.zero;
    private SerializedProperty _enemyPrefabsProp;
    private SerializedProperty _enemyParentProp;
    private SerializedProperty _battleWavesProp;
    private GUIStyle _headerStyle;
    private GUIStyle _subHeaderStyle;
    private GUIStyle _selectedStyle;
    private List<string> _enemyNames = new List<string>();
    private Vector2 _scrollPosition;

    private void OnEnable()
    {
        _battleRoom = (BattleRoom)target;
        _enemyPrefabsProp = serializedObject.FindProperty("_enemyPrefabs");
        _enemyParentProp = serializedObject.FindProperty("_enemyParent");
        _battleWavesProp = serializedObject.FindProperty("BattleWaves");
        SceneView.duringSceneGui += OnSceneGUI;
        
        // 获取所有敌人预制体名称
        UpdateEnemyNamesList();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void UpdateEnemyNamesList()
    {
        _enemyNames.Clear();
        if (_enemyPrefabsProp.arraySize == 0)
        {
            _enemyNames.Add("未添加任何敌人预制体");
            return;
        }
        
        for (int i = 0; i < _enemyPrefabsProp.arraySize; i++)
        {
            GameObject prefab = _enemyPrefabsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
            if (prefab != null)
            {
                _enemyNames.Add(prefab.name);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 初始化样式
        InitializeStyles();
        
        // 显示脚本字段
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("脚本", MonoScript.FromMonoBehaviour((BattleRoom)target), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        
        // 敌人预制体设置
        EditorGUILayout.LabelField("敌人预制体设置", _headerStyle);
        EditorGUILayout.PropertyField(_enemyPrefabsProp, new GUIContent("敌人预制体列表"));
        EditorGUILayout.PropertyField(_enemyParentProp, new GUIContent("敌人父物体"));
        
        if (GUILayout.Button("更新敌人名称列表"))
        {
            UpdateEnemyNamesList();
        }
        
        EditorGUILayout.Space(15);
        
        // 战斗波次设置
        EditorGUILayout.LabelField("战斗波次设置", _headerStyle);
        EditorGUILayout.HelpBox("单击波次或敌人在场景中查看。在2D场景中，右键单击添加新的敌人生成点。", MessageType.Info);
        
        // 添加新波次按钮
        if (GUILayout.Button("添加新的波次"))
        {
            _isCreatingNewWave = true;
            _isCreatingNewSpawn = false;
            Undo.RecordObject(_battleRoom, "添加战斗波次");
            
            SerializedProperty wavesProperty = _battleWavesProp;
            wavesProperty.arraySize++;
            int newWaveIndex = wavesProperty.arraySize - 1;
            
            // 添加空波次
            SerializedProperty newWave = wavesProperty.GetArrayElementAtIndex(newWaveIndex);
            SerializedProperty enemiesListProp = newWave.FindPropertyRelative("EnemiesThisWave");
            enemiesListProp.arraySize = 0;
            
            _selectedWaveIndex = newWaveIndex;
            _selectedSpawnIndex = -1;
            serializedObject.ApplyModifiedProperties();
        }
        
        // 显示战斗波次（使用滚动视图）
        EditorGUILayout.Space(5);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
        DisplayBattleWaves();
        EditorGUILayout.EndScrollView();
        
        // 显示战斗事件
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("战斗事件", _headerStyle);
        SerializedProperty battleStartProp = serializedObject.FindProperty("OnBattleStart");
        SerializedProperty battleEndProp = serializedObject.FindProperty("OnBattleEnd");
        SerializedProperty waveStartProp = serializedObject.FindProperty("OnWaveStart");
        SerializedProperty waveEndProp = serializedObject.FindProperty("OnWaveEnd");
        
        EditorGUILayout.PropertyField(battleStartProp, new GUIContent("战斗开始事件"));
        EditorGUILayout.PropertyField(battleEndProp, new GUIContent("战斗结束事件"));
        EditorGUILayout.PropertyField(waveStartProp, new GUIContent("波次开始事件"));
        EditorGUILayout.PropertyField(waveEndProp, new GUIContent("波次结束事件"));
        
        // 测试按钮
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (Application.isPlaying && GUILayout.Button("开始战斗", GUILayout.Height(30)))
        {
            _battleRoom.StartBattle();
        }
        
        if (Application.isPlaying && GUILayout.Button("结束战斗", GUILayout.Height(30)))
        {
            _battleRoom.ForceEndBattle();
        }
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayBattleWaves()
    {
        // 显示所有波次
        for (int waveIndex = 0; waveIndex < _battleWavesProp.arraySize; waveIndex++)
        {
            SerializedProperty waveProp = _battleWavesProp.GetArrayElementAtIndex(waveIndex);
            SerializedProperty enemiesListProp = waveProp.FindPropertyRelative("EnemiesThisWave");
            
            // 波次标题样式设置
            GUIStyle waveStyle = waveIndex == _selectedWaveIndex ? _selectedStyle : _subHeaderStyle;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            // 波次标题和移除按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"波次 {waveIndex + 1} (敌人数量: {enemiesListProp.arraySize})", waveStyle))
            {
                _selectedWaveIndex = waveIndex;
                _selectedSpawnIndex = -1;
                _isCreatingNewSpawn = false;
                SceneView.RepaintAll();
            }
            
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("移除波次", $"确定要移除波次 {waveIndex + 1} 吗?", "确定", "取消"))
                {
                    Undo.RecordObject(_battleRoom, "移除战斗波次");
                    _battleWavesProp.DeleteArrayElementAtIndex(waveIndex);
                    _selectedWaveIndex = -1;
                    _selectedSpawnIndex = -1;
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // 如果选中了当前波次，显示敌人列表
            if (waveIndex == _selectedWaveIndex)
            {
                EditorGUI.indentLevel++;
                
                // 添加敌人按钮
                if (GUILayout.Button("添加敌人生成点"))
                {
                    _isCreatingNewSpawn = true;
                    _isCreatingNewWave = false;
                    _newSpawnPosition = Vector3.zero;
                    EditorUtility.DisplayDialog("添加敌人生成点", "请在场景中右键点击以放置敌人生成点", "确定");
                    SceneView.RepaintAll();
                }
                
                // 显示敌人列表
                for (int spawnIndex = 0; spawnIndex < enemiesListProp.arraySize; spawnIndex++)
                {
                    SerializedProperty spawnProp = enemiesListProp.GetArrayElementAtIndex(spawnIndex);
                    SerializedProperty enemyNameProp = spawnProp.FindPropertyRelative("EnemyName");
                    SerializedProperty spawnLocationProp = spawnProp.FindPropertyRelative("SpawnLocation");
                    
                    // 敌人条目样式设置
                    GUIStyle spawnStyle = (waveIndex == _selectedWaveIndex && spawnIndex == _selectedSpawnIndex) ? _selectedStyle : GUI.skin.label;
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button($"敌人 {spawnIndex + 1}: {enemyNameProp.stringValue}", spawnStyle))
                    {
                        _selectedSpawnIndex = spawnIndex;
                        _isCreatingNewSpawn = false;
                        
                        // 聚焦到场景中的位置
                        if (SceneView.lastActiveSceneView != null)
                        {
                            Vector3 worldPos = _battleRoom.transform.TransformPoint(spawnLocationProp.vector3Value);
                            SceneView.lastActiveSceneView.LookAt(worldPos);
                            SceneView.lastActiveSceneView.size = 5; // 调整缩放以更好地查看
                        }
                        
                        SceneView.RepaintAll();
                    }
                    
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        if (EditorUtility.DisplayDialog("移除敌人", $"确定要移除敌人 {spawnIndex + 1} 吗?", "确定", "取消"))
                        {
                            Undo.RecordObject(_battleRoom, "移除敌人生成点");
                            enemiesListProp.DeleteArrayElementAtIndex(spawnIndex);
                            if (_selectedSpawnIndex == spawnIndex)
                            {
                                _selectedSpawnIndex = -1;
                            }
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // 如果选中当前敌人，显示详细信息
                    if (spawnIndex == _selectedSpawnIndex && waveIndex == _selectedWaveIndex)
                    {
                        EditorGUI.indentLevel++;
                        
                        // 敌人类型选择
                        int selectedEnemyIndex = -1;
                        for (int i = 0; i < _enemyNames.Count; i++)
                        {
                            if (_enemyNames[i] == enemyNameProp.stringValue)
                            {
                                selectedEnemyIndex = i;
                                break;
                            }
                        }
                        
                        EditorGUI.BeginChangeCheck();
                        selectedEnemyIndex = EditorGUILayout.Popup("敌人类型", selectedEnemyIndex, _enemyNames.ToArray());
                        if (EditorGUI.EndChangeCheck() && selectedEnemyIndex >= 0 && selectedEnemyIndex < _enemyNames.Count)
                        {
                            Undo.RecordObject(_battleRoom, "更改敌人类型");
                            enemyNameProp.stringValue = _enemyNames[selectedEnemyIndex];
                        }
                        
                        // 敌人生成位置
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = EditorGUILayout.Vector3Field("生成位置", spawnLocationProp.vector3Value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_battleRoom, "更改敌人生成位置");
                            spawnLocationProp.vector3Value = newPosition;
                        }
                        
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_battleRoom == null) return;
        
        // 处理场景视图中的交互
        Event e = Event.current;
        
        // 右键添加敌人生成点
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            // 添加新的敌人生成点
            if (_isCreatingNewSpawn && _selectedWaveIndex >= 0 && _selectedWaveIndex < _battleWavesProp.arraySize)
            {
                // 获取鼠标点击的位置
                Vector2 guiPosition = e.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
                
                // 使用2D碰撞检测
                RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
                Vector3 hitPoint;
                
                if (hit2D.collider != null)
                {
                    hitPoint = hit2D.point;
                }
                else
                {
                    // 如果没有碰撞到2D碰撞体，使用一个平面（假设游戏是在XY平面上）
                    Plane plane = new Plane(Vector3.forward, Vector3.zero);
                    float distance;
                    plane.Raycast(ray, out distance);
                    hitPoint = ray.GetPoint(distance);
                }
                
                // 转换为局部坐标
                _newSpawnPosition = _battleRoom.transform.InverseTransformPoint(hitPoint);
                
                // 获取当前选定的波次
                SerializedProperty waveProp = _battleWavesProp.GetArrayElementAtIndex(_selectedWaveIndex);
                SerializedProperty enemiesListProp = waveProp.FindPropertyRelative("EnemiesThisWave");
                
                // 添加新的敌人生成点
                serializedObject.Update();
                Undo.RecordObject(_battleRoom, "添加敌人生成点");
                
                int newIndex = enemiesListProp.arraySize;
                enemiesListProp.arraySize++;
                SerializedProperty newEnemyProp = enemiesListProp.GetArrayElementAtIndex(newIndex);
                
                SerializedProperty enemyNameProp = newEnemyProp.FindPropertyRelative("EnemyName");
                SerializedProperty spawnLocationProp = newEnemyProp.FindPropertyRelative("SpawnLocation");
                
                // 设置默认值
                enemyNameProp.stringValue = _enemyNames.Count > 0 ? _enemyNames[0] : "";
                spawnLocationProp.vector3Value = _newSpawnPosition;
                
                _selectedSpawnIndex = newIndex;
                _isCreatingNewSpawn = false;
                
                serializedObject.ApplyModifiedProperties();
                e.Use();
            }
        }
        
        // 绘制所有敌人生成点
        DrawAllSpawnPoints();
        
        // 绘制选中的敌人生成点的操作手柄
        if (_selectedWaveIndex >= 0 && _selectedWaveIndex < _battleWavesProp.arraySize &&
            _selectedSpawnIndex >= 0)
        {
            serializedObject.Update();
            
            SerializedProperty waveProp = _battleWavesProp.GetArrayElementAtIndex(_selectedWaveIndex);
            SerializedProperty enemiesListProp = waveProp.FindPropertyRelative("EnemiesThisWave");
            
            if (_selectedSpawnIndex < enemiesListProp.arraySize)
            {
                SerializedProperty spawnProp = enemiesListProp.GetArrayElementAtIndex(_selectedSpawnIndex);
                SerializedProperty spawnLocationProp = spawnProp.FindPropertyRelative("SpawnLocation");
                
                // 在场景中显示移动手柄
                EditorGUI.BeginChangeCheck();
                Vector3 worldPos = _battleRoom.transform.TransformPoint(spawnLocationProp.vector3Value);
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_battleRoom, "移动敌人生成点");
                    spawnLocationProp.vector3Value = _battleRoom.transform.InverseTransformPoint(newWorldPos);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
    
    private void DrawAllSpawnPoints()
    {
        for (int waveIndex = 0; waveIndex < _battleWavesProp.arraySize; waveIndex++)
        {
            SerializedProperty waveProp = _battleWavesProp.GetArrayElementAtIndex(waveIndex);
            SerializedProperty enemiesListProp = waveProp.FindPropertyRelative("EnemiesThisWave");
            
            // 为每个波次选择不同的颜色
            Color waveColor = new Color(
                Mathf.Sin(waveIndex * 0.7f) * 0.5f + 0.5f,
                Mathf.Sin(waveIndex * 0.4f) * 0.5f + 0.5f,
                Mathf.Sin(waveIndex * 0.3f) * 0.5f + 0.5f,
                waveIndex == _selectedWaveIndex ? 1.0f : 0.7f
            );
            
            for (int spawnIndex = 0; spawnIndex < enemiesListProp.arraySize; spawnIndex++)
            {
                SerializedProperty spawnProp = enemiesListProp.GetArrayElementAtIndex(spawnIndex);
                SerializedProperty enemyNameProp = spawnProp.FindPropertyRelative("EnemyName");
                SerializedProperty spawnLocationProp = spawnProp.FindPropertyRelative("SpawnLocation");
                
                // 获取世界坐标
                Vector3 worldPos = _battleRoom.transform.TransformPoint(spawnLocationProp.vector3Value);
                
                // 绘制连接线（如果是选中的波次）
                if (waveIndex == _selectedWaveIndex)
                {
                    if (spawnIndex > 0)
                    {
                        SerializedProperty prevSpawnProp = enemiesListProp.GetArrayElementAtIndex(spawnIndex - 1);
                        SerializedProperty prevLocationProp = prevSpawnProp.FindPropertyRelative("SpawnLocation");
                        Vector3 prevWorldPos = _battleRoom.transform.TransformPoint(prevLocationProp.vector3Value);
                        
                        Handles.color = waveColor;
                        Handles.DrawDottedLine(worldPos, prevWorldPos, 2f);
                    }
                }
                
                // 设置Gizmos颜色
                Handles.color = waveColor;
                
                // 为选中的敌人使用不同大小和颜色
                float size = (waveIndex == _selectedWaveIndex && spawnIndex == _selectedSpawnIndex) ? 0.7f : 0.4f;
                
                // 绘制圆形标记
                Handles.DrawWireDisc(worldPos, Vector3.forward, size);
                
                // 添加标签
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = waveColor;
                labelStyle.fontStyle = (waveIndex == _selectedWaveIndex && spawnIndex == _selectedSpawnIndex) ? 
                    FontStyle.Bold : FontStyle.Normal;
                
                string labelText = $"{waveIndex+1}.{spawnIndex+1}";
                if (waveIndex == _selectedWaveIndex && spawnIndex == _selectedSpawnIndex)
                {
                    labelText += $"\n{enemyNameProp.stringValue}";
                }
                
                Handles.Label(worldPos + new Vector3(0.3f, 0.3f, 0), labelText, labelStyle);
                
                // 添加按钮以选择敌人
                if (Handles.Button(worldPos, Quaternion.identity, size, size, Handles.CircleHandleCap))
                {
                    _selectedWaveIndex = waveIndex;
                    _selectedSpawnIndex = spawnIndex;
                    _isCreatingNewSpawn = false;
                    SceneView.RepaintAll();
                }
            }
        }
    }

    private void InitializeStyles()
    {
        if (_headerStyle == null)
        {
            _headerStyle = new GUIStyle(EditorStyles.boldLabel);
            _headerStyle.fontSize = 14;
            _headerStyle.margin = new RectOffset(0, 0, 10, 5);
        }
        
        if (_subHeaderStyle == null)
        {
            _subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            _subHeaderStyle.fontSize = 12;
        }
        
        if (_selectedStyle == null)
        {
            _selectedStyle = new GUIStyle(EditorStyles.boldLabel);
            _selectedStyle.normal.textColor = Color.blue;
        }
    }
} 