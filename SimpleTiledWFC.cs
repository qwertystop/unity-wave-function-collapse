using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// This is the output component of the system,
// using the simple tiled version of the algorithm
[ExecuteInEditMode]
class SimpleTiledWFC : AbstractWFC<SimpleTiledModel>{
	
	public string xmlpath = null;
	private string subset = "";

	public bool periodic = false; // Is output tiling?

	public Dictionary<string, GameObject> obmap = new Dictionary<string, GameObject>();

	public void destroyChildren (){
		foreach (Transform child in this.transform) {
     		GameObject.DestroyImmediate(child.gameObject);
 		}
 	}

 	void Start(){
		Generate();
		Run();
	}

	// Read training, and set up model and output space
	public override void Generate(){
		obmap = new  Dictionary<string, GameObject>();

		if (output == null){
			Transform ot = transform.Find("output-tiled");
			if (ot != null){output = ot.gameObject;}}
		if (output == null){
			output = new GameObject("output-tiled");
			output.transform.parent = transform;
			output.transform.position = this.gameObject.transform.position;
			output.transform.rotation = this.gameObject.transform.rotation;}

		for (int i = 0; i < output.transform.childCount; i++){
			GameObject go = output.transform.GetChild(i).gameObject;
			if (Application.isPlaying){Destroy(go);} else {DestroyImmediate(go);}
		}
		group = new GameObject(xmlpath).transform;
		group.parent = output.transform;
		group.position = output.transform.position;
		group.rotation = output.transform.rotation;
        group.localScale = new Vector3(1f, 1f, 1f);
        rendering = new GameObject[width, depth];
		this.model = new SimpleTiledModel(Application.dataPath+"/"+xmlpath, subset, width, depth, periodic);
        undrawn = true;
    }

	// Transfer model's output into rendering/worldspace
	public override void Draw(){
		if (output == null){return;}
		if (group == null){return;}
        undrawn = false;
		for (int y = 0; y < depth; y++){
			for (int x = 0; x < width; x++){ 
				if (rendering[x,y] == null){
					string v = model.Sample(x, y);
					int rot = 0;
					GameObject fab = null;
					if (v != "?"){
						rot = int.Parse(v.Substring(0,1));
						v = v.Substring(1);
						if (!obmap.ContainsKey(v)){
							fab = (GameObject)Resources.Load(v, typeof(GameObject));
							obmap[v] = fab;
						} else {
							fab = obmap[v];
						}
						if (fab == null){
							continue;}
						Vector3 pos = new Vector3(x*gridsize, y*gridsize, 0f);
						GameObject tile = (GameObject)Instantiate(fab, new Vector3() , Quaternion.identity);
						Vector3 fscale = tile.transform.localScale;
						tile.transform.parent = group;
						tile.transform.localPosition = pos;
						tile.transform.localEulerAngles = new Vector3(0, 0, 360-(rot*90));
						tile.transform.localScale = fscale;
						rendering[x,y] = tile;
					} else
                    {
                        undrawn = true;
                    }
				}
			}
  		}
	}

	// Clear an area from the grid
	public override void ClearArea(int minx, int miny, int dx, int dy) {
		Debug.Log ("Not implemented yet");
	}
	// Move the contents of the grid within the grid.
	public override void Shift(int dx, int dy){
		Debug.Log ("Not implemented yet");
	}
	// Update the model with the current contents of the grid
	public override void UpdateModel(){
		Debug.Log ("Not implemented yet");
	}
	// Continue generating without clearing the space first
	public override void Continue(){
		Debug.Log ("Not implemented yet");
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.magenta;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(
			new Vector3(width*gridsize/2f-gridsize*0.5f, depth*gridsize/2f-gridsize*0.5f, 0f),
			new Vector3(width*gridsize, depth*gridsize, gridsize));
	}
}

#if UNITY_EDITOR
[CustomEditor (typeof(SimpleTiledWFC))]
public class TileSetEditor : Editor {
	public override void OnInspectorGUI () {
		SimpleTiledWFC me = (SimpleTiledWFC)target;
		if (me.xmlpath != null){
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