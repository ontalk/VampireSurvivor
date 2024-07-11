//
// 원점에서 이동하지 않는 Mecanim 애니메이션 데이터를 사용하는 Rigidbody가 있는 콘트롤러
// 샘플
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;

namespace UnityChan
{
    // 필요한 컴포넌트 목록
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]

    public class UnityChanControlScriptWithRgidBody : MonoBehaviour
    {

        public float animSpeed = 1.5f;              // 애니메이션 재생 속도 설정
        public float lookSmoother = 3.0f;           // 카메라 이동을 위한 부드러운 설정
        public bool useCurves = true;               // Mecanim에서 곡선 조정을 사용할지 여부 설정
        // 이 스위치가 꺼져 있으면 곡선을 사용하지 않습니다.
        public float useCurvesHeight = 0.5f;        // 곡선 보정의 유효 높이(지면을 미끄럽게 통과할 때 크게 설정)

        // 캐릭터 컨트롤러 매개 변수
        // 전진 속도
        public float forwardSpeed = 7.0f;
        // 후진 속도
        public float backwardSpeed = 2.0f;
        // 회전 속도
        public float rotateSpeed = 2.0f;
        // 점프 파워
        public float jumpPower = 3.0f;
        // 캐릭터 컨트롤러(캡슐 콜라이더)의 참조
        private CapsuleCollider col;
        private Rigidbody rb;
        // 캐릭터 컨트롤러(캡슐 콜라이더)의 이동량
        private Vector3 velocity;
        // CapsuleCollider에서 설정된 콜라이더의 Heiht, Center의 초기 값 보관 변수
        private float orgColHight;
        private Vector3 orgVectColCenter;
        private Animator anim;                         // 캐릭터에 부착된 애니메이터에 대한 참조
        private AnimatorStateInfo currentBaseState;            // 베이스 레이어에서 사용되는, 현재 애니메이터 상태에 대한 참조

        private GameObject cameraObject;    // 메인 카메라에 대한 참조

        // 애니메이터 각 상태에 대한 참조
        static int idleState = Animator.StringToHash("Base Layer.Idle");
        static int locoState = Animator.StringToHash("Base Layer.Locomotion"); //달리기 모션
        static int jumpState = Animator.StringToHash("Base Layer.Jump");
        static int restState = Animator.StringToHash("Base Layer.Rest");
        static int RunState = Animator.StringToHash("Base Layer.Run");
        // 초기화
        void Start()
        {
            // Animator 컴포넌트 가져오기
            anim = GetComponent<Animator>();
            // CapsuleCollider 컴포넌트 가져오기(캡슐형 충돌)
            col = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            // 메인 카메라 가져오기
            cameraObject = GameObject.FindWithTag("MainCamera");
            // CapsuleCollider 컴포넌트의 Height, Center의 초기 값을 저장
            orgColHight = col.height;
            orgVectColCenter = col.center;
        }


        // 아래는 메인 처리입니다. 리지드바디와 결합되므로 FixedUpdate 내에서 처리합니다.
        void FixedUpdate()
        {
            float h = Input.GetAxis("Horizontal");             // 입력 장치의 수평 축을 h로 정의
            float v = Input.GetAxis("Vertical");               // 입력 장치의 수직 축을 v로 정의
            anim.SetFloat("Speed", v);                         // Animator 측에서 설정한 "Speed" 매개변수에 v를 전달
            anim.SetFloat("Direction", h);                      // Animator 측에서 설정한 "Direction" 매개변수에 h를 전달
            anim.speed = animSpeed;                             // Animator의 모션 재생 속도에 animSpeed를 설정
            currentBaseState = anim.GetCurrentAnimatorStateInfo(0);    // 참조 용 상태 변수에 Base Layer(0)의 현재 상태를 설정
            rb.useGravity = true; // 점프 중에 중력을 끄므로, 그 외에는 중력의 영향을 받도록 함

            // 아래는 캐릭터 이동 처리입니다.
            velocity = new Vector3(0, 0, v);        // 상하 키 입력에서 Z축 방향의 이동량을 가져옵니다.
            // 캐릭터의 로컬 공간에서의 방향으로 변환합니다.
            velocity = transform.TransformDirection(velocity);
            // 아래의 v의 임계값은 Mecanim 쪽의 트랜지션과 함께 조정됩니다.
            if (v > 0.1)
            {
                velocity *= forwardSpeed;       // 이동 속도를 곱합니다.
            }
            else if (v < -0.1)
            {
                velocity *= backwardSpeed;  // 이동 속도를 곱합니다.
            }

            if (Input.GetButtonDown("Jump"))    // 스페이스 키를 입력하면
            {

                // 애니메이션 상태가 Locomotion 중인 경우에만 점프할 수 있습니다.
                if (currentBaseState.nameHash == locoState || currentBaseState.nameHash == RunState)
                {
                    // 상태 전이 중이 아니면 점프할 수 있습니다.
                    if (!anim.IsInTransition(0))
                    {
                        rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                        anim.SetBool("Jump", true);      // Animator에 점프로 전환하는 플래그를 보냅니다.
                    }
                }
            }


            // 상하 키 입력으로 캐릭터를 이동시킵니다.
            transform.localPosition += velocity * Time.fixedDeltaTime;

            // 좌우 키 입력으로 캐릭터를 Y축으로 회전시킵니다.
            transform.Rotate(0, h * rotateSpeed, 0);

            // 아래는 Animator의 각 상태에서의 처리입니다.
            // Locomotion 중인 경우
            // 현재 베이스 레이어가 locoState일 때
            if (currentBaseState.nameHash == locoState)
            {
                // 곡선을 사용하여 콜라이더 조정을하고있는 경우, 리셋합니다.
                if (useCurves)
                {
                    resetCollider();
                }
            }
            // 점프 중인 경우
            // 현재 베이스 레이어가 jumpState일 때
            else if (currentBaseState.nameHash == jumpState)
            {
                // 카메라가 점프 중일 때로 변경됩니다.
                // 트랜지션 상태가 아닌 경우
                if (!anim.IsInTransition(0))
                {
                    // 아래에서 곡선 조정을하는 경우의 처리입니다.
                    if (useCurves)
                    {
                        // 아래 JumpHeight 및 GravityControl와 함께 JUMP00 애니메이션에 적용된 곡선
                        // JumpHeight: 점프의 높이 (0 ~ 1)
                        // GravityControl: 1⇒점프 중 (중력 무효), 0⇒중력 유효
                        float jumpHeight = anim.GetFloat("JumpHeight");
                        float gravityControl = anim.GetFloat("GravityControl");
                        if (gravityControl > 0)
                            rb.useGravity = false;  // 점프 중에 중력 영향을 끕니다.

                        // 캐릭터의 중심에서 레이캐스트를 떨어뜨립니다.
                        Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                        RaycastHit hitInfo = new RaycastHit();
                        // 곡선을 사용하여 콜라이더의 높이와 중심을 조정합니다.
                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            if (hitInfo.distance > useCurvesHeight)
                            {
                                col.height = orgColHight - jumpHeight;            // 조정된 콜라이더의 높이
                                float adjCenterY = orgVectColCenter.y + jumpHeight;
                                col.center = new Vector3(0, adjCenterY, 0);   // 조정된 콜라이더의 중심
                            }
                            else
                            {
                                // 임계값보다 낮을 때 초기 값으로 돌아갑니다(예방 차원에서)
                                resetCollider();
                            }
                        }
                    }
                    // Jump bool 값을 재설정합니다(반복하지 않도록).
                    anim.SetBool("Jump", false);
                }
            }
            // IDLE 상태인 경우
            // 현재 베이스 레이어가 idleState일 때
            else if (currentBaseState.nameHash == idleState)
            {
                // 곡선을 사용하여 콜라이더 조정을하고있는 경우, 리셋합니다.
                if (useCurves)
                {
                    resetCollider();
                }
                // 스페이스 키를 입력하면 Rest 상태가됩니다.
                if (Input.GetButtonDown("Jump"))
                {
                    anim.SetBool("Rest", true);
                }
            }
            // REST 상태인 경우
            // 현재 베이스 레이어가 restState일 때
            else if (currentBaseState.nameHash == restState)
            {
                // 트랜지션 중이 아닌 경우, Rest bool 값을 재설정합니다(반복하지 않도록).
                if (!anim.IsInTransition(0))
                {
                    anim.SetBool("Rest", false);
                }
            }
        }

        // 캐릭터 콜라이더 크기 재설정 함수
        void resetCollider()
        {
            // 컴포넌트의 높이 및 중심의 초기 값을 복원합니다.
            col.height = orgColHight;
            col.center = orgVectColCenter;
        }
    }
}
