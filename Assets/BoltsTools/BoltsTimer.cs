using System;
using System.Collections;
using UnityEngine;

namespace BoltsTools
{
    public class BoltsTimer
    {
        /// <summary>
        /// Freezes The Frame
        /// </summary>
        /// <param name="sec">The Number Of Seconds Frozen</param>
        public static IEnumerator FreezeFrame(float sec)
        {
            float oldTimeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(sec);
            Time.timeScale = oldTimeScale;
        }

        /// <summary>
        /// Waits Until Condition Is True
        /// </summary>
        /// <param name="T">The Condition(s)</param>
        /// <param name="F">The Action To Run</param>
        /// <returns></returns>
        public static IEnumerator WaitFor(Func<bool> T, Action F)
        {
            yield return new WaitUntil(T);
            F?.Invoke();
        }

        /// <summary>
        /// Waits To Do An Action
        /// </summary>
        /// <param name="sec">The Number Of Seconds To Wait</param>
        /// <param name="F">The Action To Run</param>
        /// <returns></returns>
        public static IEnumerator WaitForSeconds(float sec, Action F)
        {
            yield return new WaitForSeconds(sec);
            F?.Invoke();
        }

        /// <summary>
        /// Waits For Animation To Finish
        /// </summary>
        /// <param name="animator">The Animator That Is Playing The Animation</param>
        /// <param name="clipName">The Animation Clip Name</param>
        /// <param name="F">The Action To Run</param>
        /// <returns></returns>
        public static IEnumerator WaitForAnimation(Animator animator, string clipName, Action F)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(clipName))
                yield return null;

            while (animator.GetCurrentAnimatorStateInfo(0).IsName(clipName) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;
            
            F?.Invoke();
        }
    }
}
