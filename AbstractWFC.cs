using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
abstract class AbstractWFC<M> : MonoBehaviour where M : Model {
	public int gridsize = 1; // Unity units per grid square.
	public int width = 20;
	public int depth = 20;

	public int seed = 0; // for the model, for repeatability
	public int iterations = 0; // How many steps?

	public bool incremental = false; // Run only partway

	public M model; // WFC model
	public GameObject[,] rendering; // Output tile map
	public GameObject output; // Object parenting group
	protected Transform group; // Object parenting output tiles
	protected bool undrawn = true; // Is there more to do?
	
	void Update(){
		if (incremental){
			Run();
		}
	}

	// Run the model for `iterations` steps
	public void Run(){
		if (model == null){return;}
		if (undrawn == false) { return; }
		if (model.Run(seed, iterations)){
			Draw();
		}
	}

	// Read training, and set up model and output space
	public abstract void Generate();
	// Transfer model's output into rendering/worldspace
	public abstract void Draw();
}

