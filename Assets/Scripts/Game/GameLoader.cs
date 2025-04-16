using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Odyssey.Script
{
    [AddComponentMenu("Game/Game Loader")]
    public class GameLoader : Singleton<GameLoader>
    {
        /// <summary>
        /// 加载过程开始时触发
        /// </summary>
        public UnityEvent OnLoadStart;

        /// <summary>
        /// 加载过程完成时触发
        /// </summary>
        public UnityEvent OnLoadFinish;


        /// <summary>
        /// 加载界面动画控制器
        /// </summary>
        public UIAnimator loadingScreen;

        /// <summary>
        /// 加载开始前的延迟时间(秒)
        /// </summary>
        public float startDelay = 1f;
        /// <summary>
        /// 加载完成后的延迟时间(秒)
        /// </summary>
        public float finishDelay = 1f;

   
        /// <summary>
        /// 是否正在加载中
        /// </summary>
        public bool isLoading { get; protected set; }

        /// <summary>
        /// 当前加载进度(0-1)
        /// </summary>
        public float loadingProgress { get; protected set; }

        /// <summary>
        /// 当前场景名称
        /// </summary>
        public string currentScene => SceneManager.GetActiveScene().name;

        /// <summary>
        /// 重新加载当前场景
        /// </summary>
        public virtual void Reload()
        {
            StartCoroutine(LoadRoutine(currentScene));
        }

        /// <summary>
        /// 加载指定名称的场景
        /// </summary>
        /// <param name="scene">要加载的场景名称</param>
        public virtual void Load(string scene)
        {
            // 避免重复加载相同场景
            if (!isLoading && (currentScene != scene))
            {
                StartCoroutine(LoadRoutine(scene));
            }
        }

        /// <summary>
        /// 场景加载协程
        /// </summary>
        /// <param name="scene">目标场景名称</param>
        protected virtual IEnumerator LoadRoutine(string scene)
        {
            // 触发加载开始事件
            OnLoadStart?.Invoke();
            isLoading = true;

            // 显示加载界面
            loadingScreen.SetActive(true);
            loadingScreen.Show();

            // 开始延迟等待
            yield return new WaitForSeconds(startDelay);

            // 开始异步加载场景
            var operation = SceneManager.LoadSceneAsync(scene);
            loadingProgress = 0;

            // 更新加载进度
            while (!operation.isDone)
            {
                loadingProgress = operation.progress;
                yield return null;
            }

            loadingProgress = 1;

            // 完成延迟等待
            yield return new WaitForSeconds(finishDelay);

            // 恢复状态
            isLoading = false;
            loadingScreen.Hide();
            OnLoadFinish?.Invoke();
        }
    }
}