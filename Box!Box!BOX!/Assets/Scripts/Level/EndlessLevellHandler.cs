using System.Collections;
using UnityEditor.Search;
using UnityEngine;

public class EndlessLevellHandler : MonoBehaviour
{
    [SerializeField] GameObject[] sectionsPrefabs;
    GameObject[] sectionsPool = new GameObject[20]; //the pool

    GameObject[] sections = new GameObject[10]; //the sections actually rendered in the world from the pool

    Transform playerCarTransform;
    WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);

    const float sectionLength = 26;
    void Start()
    {
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0; //in case the prefabs are smaller than the sections pool, we will loop on it
        for (int i = 0; i < sectionsPool.Length; i++) {
            sectionsPool[i] = Instantiate(sectionsPrefabs[prefabIndex++]);
            sectionsPool[i].SetActive(false);

            if(prefabIndex > sectionsPrefabs.Length - 1) prefabIndex = 0;
        }

        //add the first section to the road
        for(int i = 0; i < sections.Length; i++)
        {
            GameObject randomSection = GetRandomObjectFromPool();
            
            randomSection.transform.position = new Vector3(sectionsPool[i].transform.position.x, -10, i * sectionLength);
            randomSection.SetActive(true);

            sections[i] = randomSection;
        }

        StartCoroutine(UpdateLessOfTenCO());
    }

    IEnumerator UpdateLessOfTenCO()
    {
        while (true)
        {
            UpdateSectionPosition();
            yield return waitFor100ms;
        }
    }

    void UpdateSectionPosition()
    {
        for(int i = 0; i < sections.Length; i++)
        {
            //check if the section is too far behind
            if (sections[i].transform.position.z - playerCarTransform.transform.position.z < -sectionLength)
            {
                //store the position of it and disable
                Vector3 lastSectionPosition = sections[i].transform.position;
                sections[i].SetActive(false);

                //get new section, enable and move it forward
                sections[i] = GetRandomObjectFromPool();

                sections[i].transform.position = new Vector3(lastSectionPosition.x, -10, lastSectionPosition.z + sectionLength * sections.Length);
                sections[i].SetActive(true);
            }
        }
    }

    GameObject GetRandomObjectFromPool()
    {
        int randomIndex = Random.Range(0, sectionsPool.Length);
        bool isNewSection = false;

        while(!isNewSection)
        {
            if(!sectionsPool[randomIndex].activeInHierarchy)
                isNewSection = true;
            else
            {
                randomIndex++;

                if(randomIndex > sectionsPool.Length - 1) 
                    randomIndex = 0;
            }
        }

        return sectionsPool[randomIndex];
    }
}
