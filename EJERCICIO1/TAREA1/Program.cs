using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace Tarea1{

    class Program{

        public class Paciente {
            public int Id { get; set; }

            public Paciente(int id) {
                this.Id = id;
            }
        
        }
    
        static SemaphoreSlim SalaEspera = new SemaphoreSlim(20);

        static bool[] Medicos = new bool[4]{ true, true, true,true};

        static Random rnd = new Random();

        static readonly object locker = new object();
    

        static void Main(string[] args) {
            
                for (int i = 0; i < 4; i++) {
                    int paciente = i+1;
                    Thread Medico = new Thread(() => Comportamiento(paciente));
                    Medico.Start();
                    Thread.Sleep(2000);                
                }
       
        }

        static void Comportamiento(int paciente) {

            Console.WriteLine("El paciente {0} llega a la sala de consultas.",paciente);
            SalaEspera.Wait();
            
            int medicoAsignado = 1;
            lock (locker) {
               List<int> medicosDisponibles = Medicos.Select((disponible, index) => new {disponible,index}).Where(m => m.disponible).Select(m=>m.index).ToList();
             
               if (medicosDisponibles.Count > 0) {
                   medicoAsignado = medicosDisponibles[rnd.Next(medicosDisponibles.Count)];
                   Medicos[medicoAsignado] = false;
               }
            }

            Thread.Sleep(10000);
            Console.WriteLine("El médico {0} ha acabado de atender al paciente {1}",medicoAsignado,paciente);

            lock (locker) {
               Medicos[medicoAsignado] = true;
            }
            SalaEspera.Release();

        }
    }
}