using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    private GameObject targetObject;
    private RaycastHit hit;

    [SerializeField] private GameObject blueCube;
    [SerializeField] private GameObject redCube;

    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private GameObject parent;
    private List<GameObject>[] nodesCube; // there are 2 sequences
    private List<GameObject>[] edgesCube; // 2 sequences
    int blue0Red1;

    bool paused = false;
    [SerializeField] private GameObject pauseText;
    bool moved = false;
    [SerializeField] private Button buttonMove;
    
    int score = 0;
    [SerializeField] private Text scoreText;

    int currentNodeBlue = 0;
    int currentNodeRed = 0;

    private void Start()
    {
        nodesCube = new List<GameObject>[] { new List<GameObject>(), new List<GameObject>() };
        edgesCube = new List<GameObject>[] { new List<GameObject>(), new List<GameObject>() };
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() || paused)
                return;
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f));
            Vector3 direction = worldMousePosition - Camera.main.transform.position;
            if (Physics.Raycast(Camera.main.transform.position, direction, out hit, 100f))
            {
                if (hit.collider.gameObject.name == "CubeBlue" || hit.collider.gameObject.name == "CubeRed")
                {
                    blue0Red1 = (hit.collider.gameObject.name == "CubeBlue") ? 0 : 1;
                    if (targetObject != null)
                    {
                        targetObject.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0f);
                        ResetSelection();
                    }
                    targetObject = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1f);
                }
                if (hit.collider.gameObject.name == "Plane")
                {
                    if (targetObject != null)
                    {
                        targetObject.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0f);
                        MakePathPoint(blue0Red1);
                    }
                }
            }
        }
        
        if (moved)
        {
            if (nodesCube[0].Count > 0 && currentNodeBlue < edgesCube[0].Count)
            {
                if (edgesCube[0][currentNodeBlue] != null) Destroy(edgesCube[0][currentNodeBlue]);
                blueCube.transform.position = Vector3.MoveTowards(blueCube.transform.position, nodesCube[0][currentNodeBlue].transform.position, 2f * Time.deltaTime);
                if (blueCube.transform.position == nodesCube[0][currentNodeBlue].transform.position)
                {
                    Destroy(nodesCube[0][currentNodeBlue]);
                    scoreText.text = "Score: " + (++score);
                    currentNodeBlue++;
                }
            }
            if (nodesCube[1].Count > 0 && currentNodeRed < nodesCube[1].Count)
            {
                if (edgesCube[1][currentNodeRed] != null) Destroy(edgesCube[1][currentNodeRed]);
                redCube.transform.position = Vector3.MoveTowards(redCube.transform.position, nodesCube[1][currentNodeRed].transform.position, 2f * Time.deltaTime);
                if (redCube.transform.position == nodesCube[1][currentNodeRed].transform.position)
                {
                    Destroy(nodesCube[1][currentNodeRed]);
                    scoreText.text = "Score: " + (++score);
                    currentNodeRed++;
                }
            }
        }
    }

    void ResetSelection()
    {
        targetObject = null;
    }

    void MakePathPoint(int n)
    {
        if (edgesCube[n].Count > 0)
            targetObject = nodesCube[n][(nodesCube[n].Count - 1)];
        if (targetObject == null) targetObject = (blue0Red1 == 0 ? blueCube : redCube);

        Vector3 nodePosition = Vector3.MoveTowards(hit.point, Camera.main.transform.position, 0.25f);
        nodesCube[n].Add(Instantiate(nodePrefab, nodePosition, Quaternion.identity, parent.transform));

        GameObject edge = Instantiate(edgePrefab, parent.transform);
        Vector3 pA = targetObject.transform.position;
        Vector3 pB = nodePosition;
        Vector3 between = pB - pA;
        float distance = between.magnitude;
        edge.transform.localScale = new Vector3(edge.transform.localScale.x, edge.transform.localScale.y, distance);
        edge.transform.position = pA + (between / 2.0f);
        edge.transform.LookAt(pB);
        edgesCube[n].Add(edge);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    public void TogglePause()
    {
        paused = !paused;
        pauseText.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void RemoveAllPaths()
    {
        nodesCube[0].Clear();
        nodesCube[1].Clear();
        edgesCube[0].Clear();
        edgesCube[1].Clear();
        foreach (Transform part in parent.transform) Destroy(part.gameObject);
    }

    public void Move()
    {
        moved = true;
        buttonMove.interactable = false;
    }
}
