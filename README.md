#### José Carlos Hernández Piñera C411

###### Tarea de LP. Concurrencia. 



La concurrencia es una característica de los sistemas operativos que permite realizar procesos de cómputo simultáneamente, ya sea paralelamente a través de procesadores de varios núcleos, o simulada con mecanismos de sincronización e hilos de ejecución. Uno de los principales retos de la programación concurrente es el de crear programas donde se coordinen correctamente los accesos y modificaciones a recursos compartidos, de forma qeue no se obtengan resultados inesperados. Con este fin se utilizan mecanismos para asegurar que no dos procesos ejecuten simultáneamete los segmentos de programa definidos como sección crítica, que serían precisamente los que poseen elementos comunes a varios hilos o procesos. Entre las trabas más comunes que se nos presentan a la hora del trabajo con procesos concurrentes se destacan fundamentalmente: _race conditions_, _deadlocks_, _resource reservation_; son precisamete estas problemáticas las que intentamos evitar cuando trabajamos estas cuestiones; para ello se nos presentan varios mecanismos que posibilitan lograr la sincronización sin que los errores anteriores ocurran. 

Queremos hacer notar que un concepto importante en el tema en cuestión y con el que se estará trabajando en todas las implementaciones brindadas, hacemos referencia a _Threads_, que no es más que una ejecución secuencial de instrucciones, básicamente un _thread(hilo)_ es una tarea que se ejecuta independientemente de las otras; diremos por tanto que estaremos programando concurrente cuando nuestro progrmaa tenga que lidear con la tarea de procesar o interactuar con varios hilos a la vez.



**Ejemplo de concurrencia**

```C#
 class Testing
    {
        static int x = 0;

        static void Main(string[] args)
        {
            Thread t1 = new Thread(Increment);
            Thread t2 = new Thread(Decrement);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();
        }

        static void Increment()
        {
            for (int i = 0; i < (1 << 10); ++i)
                x += 1;
        }

        static void Decrement()
        {
            for (int i = 0; i < (1 << 10); ++i)
                x -= 1;
        }

    }
```

 

Aquí quizá esperaríamos que cuando termine la ejecución del progrma la variable _x_ mantenga su valor de 0, porque por uno lado incrementamos su valor en 1 y por el otro lo decrementamos; la realidad es, sin embargo, que no podemos predecir cuál será el valor final de la variable, ya que en ningún momento estamos controlando que hilo está accediendo y en que momento a la variable, ni siquiera en que orden se esta intentando modificar esta, para ello se nos presentan una serie de mecanismos, que nos posibilitaran manejar estas cuestiones: _Semaphores, Monitors, Countdowns, Barriers_, a lo largo del trabajo presentado abordaremos los mismos y bridamos además una implementación y ejemplos sencillos del funcionamiento de estos. Se tomó para esto la clase _Semaphore_ de _System.Threading_ como base y a partir de ahí se proponen el resto de las implementaciones.



**Semaphores**:

Los semáforos permiten declarar zonas de exclusividad en las que más de un hilo puede entrar a la vez; para ello mantienen un contador que indica la cantidad de hilos que aún pueden acceder al semáforo. Se muestran dos métodos fundamentales que permiten la entrada y liberación de este: _WaitOne_ y _Release_, respectivamente. Como se dijo anteriormente la implementación que ofrece .Net para los semáforos es la que se usará de base en el proyecto. La clase _Semaphore_ de _System.Threading_ es la que se presenta en esta ocasión, esta se inicializa con dos valores enteros _maximunCount_ e _initialCount_, cada valor representa el número máximo de elementos que pueden pasar el semáforo y el valor inicial a considerar; notemos que dicho valor simpre debe ser menor que _maximunCount_ en otro caso tendremos una excepción de tipo _System.Threading.SemaphoreFullException_.  Al utilizar el método _WaitOne_ se pide acceso al semáforo y en caso de que no queden plazas disponibles se bloquea la llamda hasta que esto sea posible. Para terminar de usar un recurso o salir del semáforo se utiliza el método _Release_, que tanto puede llamarse sin parámetros para habilitar un solo puesto, o con un entrero que denota la cantidad que queremos liberar; notemos que .Net no garantiza que un hilo que haya llamado a _Release_ lo haya hecho también a _WaitOne_ previamente y esto como es obvio traerá conflictos. La clase también nos brinda otros métodos que se pueden chequear en la documentación oficial.

Presento ahora un ejemplo del **funcionamiento de los semáforos**.

```c#
    class Testing
    {
        static Semaphore s;
        static Random rnd;

        static void Main(string[] args)
        {
            s = new Semaphore(3, 3);
            rnd = new Random();

            for (int i = 0; i < 10; ++i)
            {
                Thread t = new Thread(IntoShop);
                t.Name = $"{i}";
                t.Start();
            }
        }

        static void IntoShop()
        {
            s.WaitOne();
            System.Console.WriteLine($"Customer {Thread.CurrentThread.Name} entering the store");
            Thread.Sleep(rnd.Next(1000, 2000));
            System.Console.WriteLine($"Customer {Thread.CurrentThread.Name} going out to the store");
            s.Release();
        }

    }
```

 ```bash
~$ dotnet run
Customer 1 entering the store
Customer 5 entering the store
Customer 2 entering the store
Customer 5 going out to the store
Customer 6 entering the store
Customer 1 going out to the store
Customer 0 entering the store
Customer 2 going out to the store
Customer 3 entering the store
Customer 6 going out to the store
Customer 4 entering the store
Customer 3 going out to the store
Customer 7 entering the store
Customer 0 going out to the store
Customer 8 entering the store
Customer 4 going out to the store
Customer 9 entering the store
Customer 8 going out to the store
Customer 7 going out to the store
Customer 9 going out to the store

 ```



**Monitors**:

Los monitores son otro mecanismo para garantizar la correcta sincronización y por consiguiente, acceso a regiones críticas de código, para lograr este fin se emplean llaves de bloqueos sobre objetos particulares. La clase _LpMonitor_ se presenta como estática y consta de dos métodos fundamentales _Entrer(obj)_ y _Exit(obj)_; estos posibilitan al hilo de ejecución bloquear sobre el objeto _obj_ y liberarlo respectivamente. La sincronización se logra por el hecho de que ningún proceso puede adquirir el bloqueo cuando lo tiene otro y la llamada a _Enter_ espera, por tanto, hasta que el bloqueo sea liberado. 

Para cada monitor se mantiene una referencia del hilo que tiene acceso en todo momento a la zona crítica, además de varias colas de hilos que esperan el cumplimiento de ciertas condiciones para brindar los accesos . 

Con el objetivo de garantizar esto y las demás funciones de la primitiva se provee la clase _ParticularMonitor_ que se encarga de manejar todos estos estados y garantizar el correcto funcionamiento de la estructura. 

La clase _LpMonitor_ posee un diccionario que es en realidad el centro del funcionamiento de todo, ya que este permite acceder  a partir de un objeto dado al monitor que lo supervisa, en caso claramente de existir alguno. 

```C#
static Dictionary<object, ParticularMonitor> warehouse = new Dictionary<object, ParticularMonitor>();
```

A la hora de llamar a alguno de los métodos estáticos de _LpMonitor_, se comprueba que el objeto con el que se llama esté contenido en el diccionario y de ser así se redirige el llamado al _ParticularMonitor_ correspondiente a dicho objeto, en otro caso se crea una nueva instancia de _ParicularMonitor_ y se almacena en el diccionario para futuros usos. 

```c#
public static void Enter(object obj)
        {
            mutex.WaitOne();

            if (!warehouse.ContainsKey(obj))
                warehouse.Add(obj, new ParticularMonitor());

            mutex.Release();

            warehouse[obj].Enter();
        }
```

Este es el método dentro de _LpMonitor_ que se encarga de realizar la tarea, se garantiza en siempre a través del semáforo _mutex_ que el acceso al objeto sea único en cada momento dado. 

La implementación presenta también otros métodos relevantes, dígase _Exit_, _Wait_, _Pulse_ y _PulseAll_. Estos métodos dentro de la clase _LpMonitor_ lo que hacen es más bien asegurarse que el objeto al que se le intenta aplicar el bloqueo esté dentro del diccionario, la implemetación de la lógica de funcionamiento se encuentra dentro de _ParticularMonitor_. 

Hacer referencia también a que las colas de hilo de las que se comentaba arriba son en realidad semáforos dentro de _ParticularMonitor_.

Para tener detalles del funcionamiento de la primitiva, chequear el código adjunto al informe donde se muestra la implementación.

Ejemplo del **funcionamiento de LpMonitor**:

```c#
    class Testing
    {

        static object o = new object();
        static int x = 0;

        static void Main(string[] args)
        {
            System.Console.WriteLine($"Initial value x = {x}");

            Thread t1 = new Thread(Increment);
            Thread t2 = new Thread(Decrement);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            System.Console.WriteLine($"Final value x = {x}");
        }

        static void Increment()
        {
            for (int i = 0; i < (1 << 10); ++i)
            {
                LpMonitor.Enter(o);
                x += 1;
                LpMonitor.Exit(o);
            }
        }

        static void Decrement()
        {
            for (int i = 0; i < (1 << 10); ++i)
            {
                LpMonitor.Enter(o);
                x -= 1;
                LpMonitor.Exit(o);
            }
        }

    }
```

Ahora que ya empleamos una primitiva el resultado si es que el esperamos.

```bash
~$ dotnet run
Initial value x = 0
Final value x = 0
```



**Countdowns**

La primitiva en cuestión nos permite detener la ejecución de un hilo hasta que no lleguen una cantidad de señales determinadas, en otras palabras, todo el que llame al método _Wait_ de esta clase se bloqueará hasta que no sucedan las cantidad de señales que se especificaron. 

Para garantizar una correcta implementación de la primitiva se emplearon dos semáforos de _System.Threading_, que posibilitan la simulación de las operaciones _Signal_ y _Wait_ y otro para garantizar la exclusión mutua al realizar operaciones sobre la clase _LpCountdown_. 

Se exponen también propiedades de solo lectura que ofrecen datos de la clase, _InitialCount_ que indica la cantidad de señales necesarias para desbloquear un hilo que se encuentra en espera, _CurrentCount_ que representa la cantidad de señales que faltan para desbloquear un hilo, _WaitingThreads_ que muestra la cantidad de hilos que se encuentran en espera e _IsSet_ que indica si _CurrentCount_ es o no 0. 

Para crear una instancia de la clase _LpCountdown_ se debe proporcionar de manera obligatoria un entero que representa la propiedad _InitialCount_ expuesta arriba. 

A continuación se expone una breve descripción del resto de los métodos empleados: 

Notar que en todos los métodos, que se necesita, se hace un uso adecuado del semáforo que se encarga de garantizar la exlusión mutua a al realizar las operaciones sobre la clase, pues antes de efecutar cualquier tarea se realiza un _WaitOne_ y su posterior _Release_ una vez completada(s) la(s) operación(es). Notemos que dicho semáforo solo puede manejar un hilo a la vez.

- AddCount: método que posee dos sobrecargas, la primera aumenta en uno el _CurrentCount_ mientras que la segunda recibe un entero _k_ que aumenta _CurrentCount_ k veces.
- Reset: método que posee dos sobrecargas, la primera establece todos los valores por defecto que se fijaron a la hora de la instancia de la clase, dígase las propiedades que se describen arriba y además los semáforos empleados. La segunda sobrecarga es exactamente lo mismo, lo que en esta ocasión se recibe un entero _k_ que denota un nuevo _InititalCount_ y _CurrentCount_ para nuestra instancia. 
- Signal: método que posee dos sobrecargas y se encarga de la manipulación de las señales que se comentaban al inicio. La primera sobrecarga se encarga de decrementar en uno el _Currentcount_ y en caso de que este se haga 0 de liberar cierto número de hilos, el número coincide con el valor en ese momento de _WaitingThreads_.  La segunda sobrecarga recibe un entero _k_ como parámetro y realiza exactamente lo mismo, lo único que en esta ocasión _CurrentCount_ se decrementa en _k_, si es  posible. 
- Wait: método que se encarga de bloquear el hilo actual hasta que se hayan registrado una cantidad específica de señales, si el _CurrentCount_ se presenta mayor a 0, entonces el hilo se bloquea a través del método _WaitOne_ presente en el semáforo que se use para esta operación. Además incrementamos el valor de _WaitingThreads_. 



Ejemplo del  **funcionamiento  de LpCountdown**:

```c#
    class Testing
    {
        static LpCountdown countdown = new LpCountdown(5);

        static Random rnd = new Random();

        static void Main(string[] args)
        {
            TestCountdown();
        }

        static void TestCountdown()
        {
            System.Console.WriteLine("Train arrive to the station.");
            new Thread(SetPassangers).Start();
            countdown.Wait();
            System.Console.WriteLine("Train ready.");
        }

        static void SetPassangers()
        {
            for (int i = 0; i < 5; ++i)
            {
                new Thread(CheckTicket).Start(i);
                Thread.Sleep(rnd.Next(1000, 3000));
            }
        }

        static void CheckTicket(object number)
        {
            Thread.Sleep(1000);
            System.Console.WriteLine($"Ticket ok for passanger {number}");
            countdown.Signal();
        }

    }
```

``` bash
~$ dotnet run
Train arrive to the station.
Ticket ok for passanger 0
Ticket ok for passanger 1
Ticket ok for passanger 2
Ticket ok for passanger 3
Ticket ok for passanger 4
Train ready.

```



**Barriers**

Primitiva de sincronización que permite que varios hilos trabajen simultáneamente en un algoritmo por fases. Cada hilo se ejecuta hasta que alcanza el punto de barrera en el código, se bloquea y espera a que los demás alcancen esta. La barrera representa el final de una fase de trabajo. Para asegurar esto, generalmente se guarda el número de elementos que han alcanzado la barrera y solo cuando esté se iguala al total de hilos o procesos que se pretenden ejecutar en cada fase, es que se puede continuar, en el caso de que no se haya alcanzado este valor  el hilo queda bloqueado en espera del permiso para seguir. 

La implementación que se muesta sigue los principios e ideas planteadas anteriormente. Enumeremos los detalles más importantes durante la implemtación:

- Participants: propiedad que expone el total de hilos a considerar por la estrucutra.
- CurrentPhase: registra el número de fases por las que se han transitado y por tanto la fase actual.
- Semaphore:  semáforo que controla el bloqueo de los hilos que representan los participantes.
- Mutex: semáforo que controla que dos operaciones no modifiquen parámetros de la clase a la vez.
- AddParticipant: método que se encarga de agregar participantes y devolver la fase actual.
- RemoveParticipant: método que se encarga de remover participantes.
- SignalAndWait: método fundamental de la clase. Se explica y se expone el código a continuación.

```c#
public void SignalAndWait()
        {
            this.mutex.WaitOne();
            if (RemainingParticipants > 1)
            {
                RemainingParticipants--;
                this.mutex.Release();
                semaphore.WaitOne();
            }
            else
            {
                semaphore.Release(Participants - 1);
                CurrentPhase++;
                RemainingParticipants = Participants;
                this.mutex.Release();
            }
        }
```

El método está controlado en todo momento por mutex, para asegurar que solo tenga acceso al código un hilo a la vez y que no se produzcan resultados inesperados. Realmente el método es sencillo de comprender solo se valida si todos los participanten han llegado a la barrera para en caso positivo liberar todas las posiciones del semáforo y proceder al cambio de fase. 

Aunque no se presenta en el código aquí expuesto, en la implementación se dio la posibilidad de pasar un delegado que permite ejecutar cierta fución una vez que todos los participantes alcancen la barrera.



Ejemplo del **funcionamiento de LpBarrier**:

```c#
 class Testing
    {
        static LpBarrier barrier = new LpBarrier(3);

        static Random rnd = new Random();

        static string[] tasks = { "Task A", "Task B", "Task C" };

        static void Main(string[] args)
        {
            TestBarrier();
        }

        static void TestBarrier()
        {
            new Thread(ProcessTask).Start();
            new Thread(ProcessTask).Start();
            new Thread(ProcessTask).Start();
            new Thread(ProcessTask).Start();
        }

        static void ProcessTask()
        {
            for (int i = 0; i < 3; ++i)
            {
                System.Console.WriteLine($"Complete task {i}");
                barrier.SignalAndWait();
            }
        }

    }
```

```bash
~$ dotnet run
Complete task A
Complete task A
Complete task A
Complete task A
Complete task B
Complete task B
Complete task B
Complete task C
Complete task B
Complete task C
Complete task C
Complete task C

```



**Problema de los 5 filósofos**

Se tiene cinco filósofos sentados alrededor de una mesa. Cada filósofo tiene un tenedor a cada lado (uno a la izquierda y otro a la derecha). Para poder comer es necesario tener dos tenedores que solo pueden ser los que están a su lado. Si un filósofo toma un tenedor y el otro está ocupado se quedará esperando con el tenedor en la mano hasta que el otro sea liberado por otro filósofo para comenzar a comer. Cuando un filósofo termina de comer libera ambos tenedores. En el caso en que todos los filósofos tomen el tenedor a su izquierda (o la derecha) a la vez se produce un bloqueo mutuo (deadlock) y los filósofos morirán de hambre. El objetivo es encontrar una soluci´on donde ningún filósofo muera de hambre.

Para la solución del problema se emplearon dos clases, una para reprsentar a los filósofos y otra a los tenedores. Cada instancia de filósofo tiene un nombre, dos instancias de tenedores y un random que se usa para determinar el tiempo que pasa pensando, comiendo e intentando coger cada tenendor.

La clase tenedor solo tiene el número del tenedor que representa. 

Antes de comenzar la simulación se crean los 5 filósofos y además 5 tenedores que serán los que se utilicen durante el exprimento.

Cada filósofo cuenta con el método _Simulate_ que se encarga de cambiar los estados de la siguiente forma: inicialmente cada uno empieza pensando por un determinado tiempo, luego  intenta por un tiempo aleatorio tomar el tenedor de la izquierda y de ser posible intenta posteriormente, por un tiempo aleatorio también, tomar el tenedor de la derecha; una vez que posea los dos tenedores pasa a comer por un período de tiempo aleatorio y al finalizar libera los dos tenedores. El ciclo se repite indefinidamente.

Para la implementación nos auxiliamos de la clase _Monitor_ que nos brinda _System.Threading_ y su método _TryEnter_ que intenta realizar el método _Enter_ por un determinado tiempo. Cada filósofo se ejecuta por un hilo diferente, llamando por aquí al método _Simulate_ explicado anteriormente. Nos auxiliamos además del método _Sleep_ presente en _Thread_ para simular el tiempo que se pasa en cada estado. 

Notar que aquí no es posible nunca que ocurra  deadlock, pues supongamos que tenemos el tenedor de la izquierda y que cuando intentamos tomar el de la derecha este está ocupado, como este segundo proceso se realiza por un tiempo determinado y al finalizar si no se puedo se libera el de la izquierda entonces aseguramos con ello el correcto funcionamiento del algoritmo.



**El barbero dormilón**

En una barbería trabaja un barbero que tiene un único sillón de barbero y varias sillas para esperar. Cuando no hay clientes, el barbero se sienta en una silla y se duerme. Cuando llega un nuevo cliente, este o bien despierta al barbero o si el barbero está pelando a otro cliente se sienta en una silla, o se va si todas las sillas están ocupadas por clientes esperando. Supongamos que el barbero está pelando y llega un cliente, el cliente ve al barbero pelando y se dirige a las sillas a esperar, justo en ese momento, el barbero termina de pelar, mira hacia las sillas y como aún no hay ningún cliente esperando, se duerme, luego el cliente se sienta en la silla a esperar. Ambos quedan bloqueados esperando (deadlock).

Para la solución del problema en cuestión nos apoyamos en el uso de la clase _Semaphore_ presente en _System.Threading_; el problema principal radica en garantizar que el barbero y el cliente no se muevan al mismo tiempo, es decir no intenten realizar ninguna operación simultánea sobre la cola de los clientes, ya sea tanto de consulta como de modificación. Un empleo sencillo de los semáforos nos garantiza el resultado que esperamos.  En el problema generamos clientes de forma aleatoria e intentamos ponerlos en la cola de espera, de ser posible, y ya sencillamente evitando que se intente operar sobre la estructura simultáneamente solucionamos la cuestión.

