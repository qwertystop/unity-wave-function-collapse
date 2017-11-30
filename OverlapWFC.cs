using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// This is the output component of the system,
// using the overlapping version of the algorithm
[ExecuteInEditMode]
class OverlapWFC : AbstractWFC<OverlappingModel>{
	public Training training = null; // Training area
	public int N = 2; // Features are NxN sections of training area
	public bool periodicInput = false; // Input is tiling (wrap edges)
	public bool periodicOutput = false; // Output must tile
	public int symmetry = 1; // Symmetries to use. 0: None. 1: Rotation-specific tiles. 2-8: Undocumented.
	public int foundation = 0; // Treat the bottom/"ground" differently (I think?)

	public static bool IsPrefabRef(UnityEngine.Object o){
		#if UNITY_EDITOR
		return PrefabUtility.GetPrefabParent(o) == null && PrefabUtility.GetPrefabObject(o) != null;
		#else
		return true;
		#endif
	}

	static GameObject CreatePrefab(UnityEngine.Object fab, Vector3 pos, Quaternion rot) {
		#if UNITY_EDITOR
		GameObject o = PrefabUtility.InstantiatePrefab(fab as GameObject) as GameObject; 
		#else
		GameObject o = GameObject.Instantiate(fab as GameObject) as GameObject;
		#endif
		o.transform.position = pos;
		o.transform.rotation = rot;
		return o;
	}

	// Clear the output space to make way for new output
	public void Clear(){
		if (group != null){
			if (Application.isPlaying){Destroy(group.gameObject);} else {
				DestroyImmediate(group.gameObject);
			}	
			group = null;
		}
	}

	void Awake(){}

	void Start(){
		Generate();
	}

	// Read training, and set up model and output space
	public override void Generate() {
		if (training == null){Debug.Log("Can't Generate: no designated Training component");}
		if (IsPrefabRef(training.gameObject)){
			GameObject o = CreatePrefab(training.gameObject, new Vector3(0,99999f,0f), Quaternion.identity);
			training = o.GetComponent<Training>();
		}
		if (training.sample == null){
			training.Compile();
		}
		if (output == null){
			Transform ot = transform.Find("output-overlap");
			if (ot != null){output = ot.gameObject;}}
		if (output == null){
			output = new GameObject("output-overlap");
			output.transform.parent = transform;
			output.transform.position = this.gameObject.transform.position;
			output.transform.rotation = this.gameObject.transform.rotation;}
		for (int i = 0; i < output.transform.childCount; i++){
			GameObject go = output.transform.GetChild(i).gameObject;
			if (Application.isPlaying){Destroy(go);} else {DestroyImmediate(go);}
		}
		group = new GameObject(training.gameObject.name).transform;
		group.parent = output.transform;
		group.position = output.transform.position;
		group.rotation = output.transform.rotation;
        group.localScale = new Vector3(1f, 1f, 1f);
        rendering = new GameObject[width, depth];
		model = new OverlappingModel(training.sample, N, width, depth, periodicInput, periodicOutput, symmetry, foundation);
        undrawn = true;
    }

	// Indexes into rendering. Unused?
	public GameObject GetTile(int x, int y){
		return rendering[x,y];
	}

	// Transfer model's output into rendering/worldspace
	public override void Draw(){
		if (output == null){return;}
		if (group == null){return;}
        undrawn = false;
		try{
			for (int y = 0; y < depth; y++){
				for (int x = 0; x < width; x++){
					if (rendering[x,y] == null){
						int v = (int)model.Sample(x, y);
						if (v != 99 && v < training.tiles.Length){
							Vector3 pos = new Vector3(x*gridsize, y*gridsize, 0f);
							int rot = (int)training.RS[v];
							GameObject fab = training.tiles[v] as GameObject;
							if (fab != null){
								GameObject tile = (GameObject)Instantiate(fab, new Vector3() , Quaternion.identity);
								Vector3 fscale = tile.transform.localScale;
								tile.transform.parent = group;
								tile.transform.localPosition = pos;
								tile.transform.localEulerAngles = new Vector3(0, 0, 360 - (rot * 90));
								tile.transform.localScale = fscale;
								rendering[x,y] = tile;
							}
						} else
                        {
                            undrawn = true;
                        }
					}
				}
	  		}
	  	} catch (IndexOutOfRangeException e) {
			Debug.Log (e.ToString ());
			model = null;
	  		return;
	  	}
	}

	// Outlines output space when active in editor
	void OnDrawGizmos(){
		Gizmos.color = Color.cyan;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(
			new Vector3(width*gridsize/2f-gridsize*0.5f, depth*gridsize/2f-gridsize*0.5f, 0f),
			new Vector3(width*gridsize, depth*gridsize, gridsize));
	}
}

 #if UNITY_EDITOR
[CustomEditor (typeof(OverlapWFC))]
public class WFCGeneratorEditor : Editor {
	public override void OnInspectorGUI () {
		OverlapWFC me = (OverlapWFC)target;
		if (me.training != null){
			if(GUILayout.Button("generate")){
				me.Generate();
			}
			if (me.model != null){
				if(GUILayout.Button("RUN")){
					me.Run();
				}
			}
		}
		DrawDefaultInspector ();
	}
}
#endif