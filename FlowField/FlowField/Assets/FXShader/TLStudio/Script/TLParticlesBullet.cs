using UnityEngine;

namespace TLStudio
{
    [RequireComponent(typeof(ParticleSystem))]
    public class TLParticlesBullet : MonoBehaviour {
    	ParticleSystem ps;
    	ParticleSystem.Particle[] m_Particles;
    	public Transform []target;
        
        ParticleSystem [] PSs;

        //[Range(0.1f, 4f)]
        //public float speed = 1f;
        //[Range(0.0f, 1f)]
        private float kill = 0.5f;

        //public float smoothTimeUse = 2;
        public float Delaytime = 0.6f;
        public float MaxDistance = 10;

        public bool Burst = false;
        [Range(1,5)]
        public int BurstCount = 3;
        public float BurstDelay = 0.5f;


        int numParticlesAlive;
        void OnEnable () {
            if (!GetComponent<Transform>())
            {
                GetComponent<Transform>();
            }
    
            
            PSs = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < PSs.Length; i++)
            {
                if (Burst)
                {
                    //PSs[i].emission.SetBursts(
                    //new ParticleSystem.Burst[]{
                    //new ParticleSystem.Burst(0.0f, target.Length)
                    //,new ParticleSystem.Burst(0.0f, target.Length)
                    //    });
                    var burst = new ParticleSystem.Burst[BurstCount];
                    for (int k = 0; k < BurstCount; k++)
                    {
                        burst[k] = new ParticleSystem.Burst(k * BurstDelay, target.Length);
                    }
                    PSs[i].emission.SetBursts(burst);
                }
                else
                {
                    PSs[i].emission.SetBursts(
                    new ParticleSystem.Burst[]{
                    new ParticleSystem.Burst(0.0f, 0)
                        });
                }
            }
        }
        
    	void Update ()
        {
            for(int i = 0; i < PSs.Length; i++)
            {
                if (m_Particles == null)
                {
                    m_Particles = new ParticleSystem.Particle[PSs[i].main.maxParticles];
                }
                Vector3 velocity;
                numParticlesAlive = PSs[i].GetParticles(m_Particles);


                for (int j = 0; j < numParticlesAlive; j++)
                {
    
                    velocity = m_Particles[j].velocity;
                    //if(m_Particles[i].remainingLifetime < 4.5f)
                    //{
                    //    maxSpeed = 7f;
                    //}
                    //else
                    //{
                    //    maxSpeed = 2f;
                    //}
                    //到达目标点kill粒子
                    float Dis;
                    if (Burst)
                    {
                        Dis = Vector3.Distance(m_Particles[j].position, target[j % target.Length].position);
                    }
                    else
                    {
                        Dis = Vector3.Distance(m_Particles[j].position, target[0].position);
                    }

                    if (Burst)
                    {
                        m_Particles[j].position = Vector3.SmoothDamp(m_Particles[j].position, target[j % target.Length].position, ref velocity, Delaytime * Dis / MaxDistance);
                    }
                    else
                    {
                        m_Particles[j].position = Vector3.SmoothDamp(m_Particles[j].position, target[0].position, ref velocity, Delaytime * Dis / MaxDistance);
                    }

                    m_Particles[j].velocity = velocity;

                    
    
                    if (Dis < kill)
                    {
                        m_Particles[j].remainingLifetime = 0.001f;
                    }
                }
    
                PSs[i].SetParticles(m_Particles, numParticlesAlive);
            }
    	}
    }
}
