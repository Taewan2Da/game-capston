using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("[Core]")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("[Object Pooling]")]
    public GameObject mococoPrefab;
    public Transform mococoGroup;
    public List<Mococo> mococoPool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Mococo lastMococo;

    [Header("[Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;

    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("[UI]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text MaxscoreText;
    public Text subScoreText;

    [Header("[ETC]")]
    public GameObject line;
    public GameObject bottom;



    void Awake()
    {
        Application.targetFrameRate = 60;

        mococoPool = new List<Mococo>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeMococo();
        }

        if (!PlayerPrefs.HasKey("MaxScore")) {
            PlayerPrefs.SetInt("MaxScore", 0);
        }


        MaxscoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }


    public void GameStart()
    {
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        MaxscoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextMococo", 1.5f);
    }

    Mococo MakeMococo()
    {
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantMococoObj = Instantiate(mococoPrefab, mococoGroup);
        instantMococoObj.name = "Mococo " + mococoPool.Count;
        Mococo instantMococo = instantMococoObj.GetComponent<Mococo>();
        instantMococo.manager = this;
        instantMococo.effect = instantEffect;
        mococoPool.Add(instantMococo);

        return instantMococo;
    }

    Mococo GetMococo()
    {
        for(int index = 0; index < mococoPool.Count; index++)
        {
            poolCursor = (poolCursor + 1) % mococoPool.Count;
            if (!mococoPool[poolCursor].gameObject.activeSelf)
            {
                return mococoPool[poolCursor];
            }
        }

       return MakeMococo();
    }

    void NextMococo()
    {
        if (isOver){
            return;
        }
        
        lastMococo = GetMococo();

        int randomLevel;

       
        if (maxLevel < 3)
        {
            randomLevel = Random.Range(0, 2);
        }
        else
        {
            randomLevel = Random.Range(0, 3);
        }

        lastMococo.level = randomLevel;
        lastMococo.gameObject.SetActive(true);
        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }



    IEnumerator WaitNext()
    {
        while (lastMococo != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        NextMococo();

    }


    public void TouchDown()
    {
        if (lastMococo == null) 
        {
            return;
        }

        lastMococo.Drag();
    }

    public void TouchUp()
    {
        if (lastMococo == null)
        {
            return;
        }
        lastMococo.Drop();
        lastMococo = null;
    }

    public void GameOver()
    {
        if(isOver) {
            return;       
        }

        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        Mococo[] mococos = FindObjectsOfType<Mococo>();

        for (int index = 0; index < mococos.Length; index++)
        {
            mococos[index].rigid.simulated = false;
            
        }

        for (int index = 0; index < mococos.Length; index++)
        {
            mococos[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        subScoreText.text = "Á¡¼ö : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
        
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;

            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;

            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;

            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;

            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
