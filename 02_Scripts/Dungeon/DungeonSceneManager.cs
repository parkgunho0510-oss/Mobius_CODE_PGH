using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonSceneManager : MonoBehaviour
{
    public static DungeonSceneManager Instance { get; private set; }

    [Header("던전 시작 좌표")]
    public Vector3 dungeonStartPos = new Vector3(0, 3, 0); // 인스펙터에서 수정 가능하도록 빼둠

    private PlayerController _player;
    private string _preSceneName;
    private Vector3 _prePosition;
    private enum TransitionState { None, EnteringDungeon, ReturningToField }
    private TransitionState _currentState = TransitionState.None;
    public bool IsReturningFromDungeon => _currentState == TransitionState.ReturningToField;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 씬이 로드되고 플레이어가 자신을 등록할 때 실행됨
    public void DSMRegisterPlayer(PlayerController playerTransform)
    {
        _player = playerTransform;

        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (_currentState == TransitionState.EnteringDungeon)
        {
            _player.transform.position = dungeonStartPos;
            _currentState = TransitionState.None; // 이동 완료 후 상태 초기화
        }
        else if (_currentState == TransitionState.ReturningToField)
        {
            _player.transform.position = _prePosition;
            StartCoroutine(ResetStat());

        }

        // 이동이 끝난 후 컨트롤러 다시 켜기
        if (cc != null) cc.enabled = true;
    }
    private IEnumerator ResetStat()
    {
        yield return new WaitForEndOfFrame();
        _currentState = TransitionState.None;
    }
    public void GoToScene(string dungeonSceneName)
    {
        if (_player != null)
        {
            // 현재(평지) 씬 이름과 위치를 저장
            _preSceneName = SceneManager.GetActiveScene().name;
            _prePosition = _player.transform.position;

            // 상태를 '던전 진입'으로 변경
            _currentState = TransitionState.EnteringDungeon;

            StartCoroutine(LoadSceneAsyncCoroutine(dungeonSceneName));
        }
    }

    public void ReturnToScene()
    {
        if (!string.IsNullOrEmpty(_preSceneName))
        {
            // 상태를 '평지 복귀'로 변경
            _currentState = TransitionState.ReturningToField;

            StartCoroutine(LoadSceneAsyncCoroutine(_preSceneName));
        }
    }

    IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }
    }
}