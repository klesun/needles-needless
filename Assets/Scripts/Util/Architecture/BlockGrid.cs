﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util.Logic;

namespace Util
{
    [ExecuteInEditMode]
    [SelectionBase]
    public class BlockGrid : MonoBehaviour
    {
        const float REVALIDATION_PERIOD = 0.1f;

        public TransformListener endPoint;
        public TransformListener blockRef;
        public GameObject blockCont;
        // these range limitations are from head.
        // feel free to increase bounds if you need
        [Range(0.01f, 1000)]
        public float spacingZ = 1.0f;
        [Range(0.01f, 1000)]
        public float spacingX = 1.0f;
        [Range(0, 100)]
        public int sideRows = 3;
        [Range(0, 1)]
        public float skipRate = 0;
        public int randomSeed = 13;

        double? lastValidatedOn = null;
        bool revalidationRequested = false;

        void Awake ()
        {
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            spacingZ = Mathf.Max (spacingZ, 0.1f);
            spacingX = Mathf.Max (spacingX, 0.1f);

            if (endPoint != null && blockRef != null && blockCont != null) {
                UnityEditor.EditorApplication.delayCall += () => revalidationRequested = true;
                endPoint.onChange = () => revalidationRequested = true;
                blockRef.onChange = () => revalidationRequested = true;
            }
        }
        #endif

        void Update()
        {
            var now = System.DateTime.Now.Ticks / 10000000d;
            if (revalidationRequested && (lastValidatedOn == null || now - lastValidatedOn > REVALIDATION_PERIOD)) {
                revalidationRequested = false;
                lastValidatedOn = now;
                Renew ();
            }
        }

        void Renew()
        {
            if (this == null) {
                // well, it complains about it being
                // destroyed when i starts the game
                return;
            }
            if (Application.isPlaying) {
                return;
            }

            var deadmen = new List<GameObject>();
            foreach (Transform ch in blockCont.transform) {
                deadmen.Add (ch.gameObject);
            }
            deadmen.ForEach (DestroyImmediate);

            blockCont.transform.LookAt (endPoint.transform);

            var random = new System.Random (randomSeed);

            var dist = Vector3.Distance (blockCont.gameObject.transform.position, endPoint.gameObject.transform.position);
            for (var i = 0; i < dist / spacingZ; ++i) {
                for (var j = -sideRows; j <= sideRows; ++j) {
                    if (random.NextDouble() < skipRate) {
                        continue;
                    }
                    var block = Instantiate (blockRef.gameObject);
                    block.name = "_block" + i + "x" + j;
                    block.transform.SetParent (blockCont.transform);

                    var wasAngles = block.transform.rotation.eulerAngles;
                    block.transform.localRotation = Quaternion.identity;
                    var nowAngles = block.transform.rotation.eulerAngles;
                    // cont always faces to end point, but we want to preserve vertical angle of segments
                    block.transform.rotation = Quaternion.Euler(wasAngles.x, nowAngles.y, nowAngles.z);

                    block.transform.localPosition = new Vector3 (j * spacingX, 0, i * spacingZ);
                }
            }
        }
    }
}