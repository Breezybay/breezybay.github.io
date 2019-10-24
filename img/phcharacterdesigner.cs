// By Zane Arn (zhanstudio.com)
// License: CC Zero (no copy right)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PHCharacterDesigner : MonoBehaviour {

    public SkinnedMeshRenderer[] skins;
    public SkinnedMeshRenderer[] hairs;
    public SkinnedMeshRenderer[] eyebrows;
    public SkinnedMeshRenderer[] eyes;
    public SkinnedMeshRenderer[] mouths;
    public SkinnedMeshRenderer[] clothes;
    public SkinnedMeshRenderer[] pants;
    //public PHElement[] accessories;
    private SkinnedMeshRenderer[][] elements;

    public SkinnedMeshRenderer[] skinheads; // for head icon

    // character configuration array, 
    // [0 skin,1 hairs, 2 eyebrow, 3 eye, 4 mouth, 5 cloth, 6 pants, 7 accessories]
    public byte[] config;// = new byte[]{1, 0, 0, 0, 0, 0, 0};
    byte[] GetCurrentConfig() { return config; }

    public GameObject root;
	
	public bool near;
	public List<Material> nearMatList;

    private List<Material> materialList;

    private List<BoneWeight> boneWeightsList = new List<BoneWeight>();

    //private float startTime;
    private float deltaTime;

	// Use this for initialization
	void Start () {

//        float startTime = Time.realtimeSinceStartup;

        config = new byte[]{0, 0, 0, 0, 0, 0, 0};

        List<SkinnedMeshRenderer[]> elementsList = new List<SkinnedMeshRenderer[]>();
        elementsList.Add(skins);
        elementsList.Add(hairs);
        elementsList.Add(eyebrows);
        elementsList.Add(eyes);
        elementsList.Add(mouths);
        elementsList.Add(clothes);
        elementsList.Add(pants);
        //elementsList.Add(accessories);
        elements = elementsList.ToArray();

    }

	// Update is called once per frame
	void Update () 
    {
    }



    // Generate a character using current config
    GameObject Generate()
    {
        GameObject go = (GameObject)Object.Instantiate(root);
        return GenerateWithConfig(go, config);
    }

    // Generate a character using random config
    public GameObject GenerateWithRandomConfig()
    {
        GameObject go = (GameObject)Object.Instantiate(root);

        return GenerateWithConfig(go, GenerateConfig());
    }
	
	public byte[] GenerateConfig()
	{
		config = new byte[] { 
            (byte)Random.Range(0, elements[0].Length), 
            (byte)Random.Range(0, elements[1].Length), 
            (byte)Random.Range(0, elements[2].Length), 
            (byte)Random.Range(0, elements[3].Length), 
            (byte)Random.Range(0, elements[4].Length), 
            (byte)Random.Range(0, elements[5].Length), 
            (byte)Random.Range(0, elements[6].Length), };
		return config;
	}
	public byte GetElementLength(int index)
	{
		if(index<0||index>6)
		{
			return 0;
		}
		return (byte)(elements[index].Length);
	}


    // Generate a character using specific config
    public GameObject GenerateWithConfig(byte[] config)
    {
        GameObject go = (GameObject)Object.Instantiate(root);
        return GenerateWithConfig(go, config);
    }
    GameObject GenerateWithConfig(GameObject go, byte[] config)
    {
        float startTime = Time.realtimeSinceStartup;

        if (!go.GetComponent<SkinnedMeshRenderer>()) { go.AddComponent<SkinnedMeshRenderer>(); }
        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        materialList = new List<Material>();
        materialList.Add(smr.material);
        boneWeightsList = new List<BoneWeight>();
        Transform[] bones = smr.bones;//bones.GetComponentsInChildren<Transform>();


        for (int i = 0; i < config.Length; i++ )
        {
            //            Debug.Log("i = " + i + ", config[i] = " + config[i] + ".");
            SkinnedMeshRenderer smr1 = (SkinnedMeshRenderer)Object.Instantiate(elements[i][config[i]]);

            // Add meshes to combineinstances
            for (int sub = 0; sub < smr1.sharedMesh.subMeshCount; sub++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = smr1.sharedMesh;
                ci.transform = smr1.transform.localToWorldMatrix;
                ci.subMeshIndex = sub;
                combineInstances.Add(ci);

                foreach (Material mat in materialList)
                {
                    if (mat.name != smr1.material.name)
                    {
                        materialList.Add(smr1.material);
                    }
                }   
            }

            boneWeightsList.AddRange(smr1.sharedMesh.boneWeights);

            Object.Destroy(smr1.gameObject);
        }

        // Combine meshes
        smr.sharedMesh = new Mesh();
        smr.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        smr.sharedMesh.boneWeights = boneWeightsList.ToArray();

        smr.bones = bones;
		
		if (near)
			smr.materials = nearMatList.ToArray();
		else
        	smr.materials = materialList.ToArray();

        // Update bindposes
        List<Matrix4x4> bindposes = new List<Matrix4x4>();
        for (int i = 0; i < bones.Length; i++)
        {
            bindposes.Add(bones[i].worldToLocalMatrix * transform.localToWorldMatrix);
        }
        smr.sharedMesh.bindposes = bindposes.ToArray();

//        Debug.Log("Generating Character took: " + (Time.realtimeSinceStartup - startTime) * 1000 + "ms");

        go.transform.Rotate(Vector3.right, -90);

        go.GetComponent<Animation>().Play();

        return go;
    }


    // Generate a head using specific config
    public GameObject GenerateProtraitWithConfig(byte[] config)
    {
        GameObject go = (GameObject)Object.Instantiate(root);
        return GenerateProtraitWithConfig(go, config);
    }
    GameObject GenerateProtraitWithConfig(GameObject go, byte[] config)
    {
        if (!go.GetComponent<SkinnedMeshRenderer>()) { go.AddComponent<SkinnedMeshRenderer>(); }
        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        materialList = new List<Material>();
        materialList.Add(smr.material);
        boneWeightsList = new List<BoneWeight>();
        Transform[] bones = smr.bones;//bones.GetComponentsInChildren<Transform>();

        for (int i = 0; i < config.Length; i++)
        {
            if (i == 5 || i == 6) // cloth and pants
            {
                continue; // skip
            }

            SkinnedMeshRenderer smr1;
            if (i == 0) //skin
            {
                smr1 = (SkinnedMeshRenderer)Object.Instantiate(skinheads[config[i]]); // Instantiate a head only
            }
            else
            {
                smr1 = (SkinnedMeshRenderer)Object.Instantiate(elements[i][config[i]]);
            }

            // Add meshes to combineinstances
            for (int sub = 0; sub < smr1.sharedMesh.subMeshCount; sub++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = smr1.sharedMesh;
                ci.transform = smr1.transform.localToWorldMatrix;
                ci.subMeshIndex = sub;
                combineInstances.Add(ci);

                foreach (Material mat in materialList)
                {
                    if (mat.name != smr1.material.name)
                    {
                        materialList.Add(smr1.material);
                    }
                }
            }

            boneWeightsList.AddRange(smr1.sharedMesh.boneWeights);

            Object.Destroy(smr1.gameObject);
        }

        // Combine meshes
        smr.sharedMesh = new Mesh();
        smr.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        smr.sharedMesh.boneWeights = boneWeightsList.ToArray();

        smr.bones = bones;
        smr.materials = materialList.ToArray();

        // Update bindposes
        List<Matrix4x4> bindposes = new List<Matrix4x4>();
        for (int i = 0; i < bones.Length; i++)
        {
            bindposes.Add(bones[i].worldToLocalMatrix * transform.localToWorldMatrix);
        }
        smr.sharedMesh.bindposes = bindposes.ToArray();

        //        Debug.Log("Generating Character took: " + (Time.realtimeSinceStartup - startTime) * 1000 + "ms");

        go.transform.Rotate(Vector3.right, -90);

        //go.GetComponent<Animation>().Play();

        return go;
    }
}
