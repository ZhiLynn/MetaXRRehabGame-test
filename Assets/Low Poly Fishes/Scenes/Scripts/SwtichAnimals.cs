using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwtichAnimals : MonoBehaviour
{
    public GameObject[] animals;
    public Text stateName;
    private AnimatorClipInfo[] m_CurrentClipInfo;
    private Animator animator;
    private int i;
    // Start is called before the first frame update
    void Start()
    {
        i = 0;
        NextAnimal();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("END"))
        {
            animator = null;
            animals[i].SetActive(false);
            i++;
            if (i > animals.Length-1)
            {
                i = 0;
            }
            NextAnimal();
        }

        m_CurrentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (animator != null && m_CurrentClipInfo.Length > 0)
        {
            stateName.text = m_CurrentClipInfo[0].clip.name;
        }
    }

    public void NextAnimal()
    {
        animator = animals[i].GetComponent<Animator>();
        animals[i].SetActive(true);
    }
}
