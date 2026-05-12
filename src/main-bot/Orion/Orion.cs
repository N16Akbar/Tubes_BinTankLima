/* TODO: 
1. Perbaiki agar firePower lebih dinamis dan akurat berdasarkan jarak musuh dan energi bot saat ini 
2. Tank harus mengelilingi musuh atau zigzag (tidak terlalu besar) saat mengarah ke musuh
*/
// Library yang dibutuhkan untuk Robocode
using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Orion : Bot
{
    // Variable untuk menyimpan sudut enemy yang di-scan berdasarkan posisi bot saat ini
    double directionToEnemy;
    // Variable untuk menyimpan jarak enemy yang di-scan berdasarkan posisi bot saat ini
    double distanceToEnemy;
    // Variable untuk menyimpan riwayat terbaru enemy yang di-scan
    double recentEnemyX, recentEnemyY;
    // Variabel untuk menandakan apakah sudah memiliki riwayat musuh atau belum
    bool hasEnemyHistory = false;

    /*
        double lastAccurateFactor = -1;

        double sumShots = 0;
        double sumHits = 0;
        double sumMiss = 0;
    */

    // Memulai objek bot pada main program
    static void Main(string[] args)
    {
        new Orion().Start();
    }

    // Memuat info bot dari Orion.json
    Orion() : base(BotInfo.FromFile("Orion.json")) { }

    public override void Run()
    {
        // Membuat radar independen dari gun saat berputar
        AdjustRadarForGunTurn = true;
        // Membuat gun independen dari body saat berputar
        AdjustGunForBodyTurn = true;

        // Warna tank
        // BodyColor = Color.Gold;
        // TurretColor = Color.Black;
        // RadarColor = Color.DarkRed;
        // BulletColor = Color.Red;
        // GunColor = Color.Goldenrod;
        BodyColor = Color.MidnightBlue;
        TurretColor = Color.Gold;
        RadarColor = Color.DarkRed;
        BulletColor = Color.OrangeRed;
        GunColor = Color.Goldenrod;

        while (IsRunning)
        {
            // Melakukan scanning radar sebesar 90 derajat per tick
            TurnRadarLeft(90);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Memasukkan nilai direction dari musuh yang ter-scan
        directionToEnemy = DirectionTo(evt.X, evt.Y);
        // Memasukkan nilai distance dari musuh yang ter-scan
        distanceToEnemy = DistanceTo(evt.X, evt.Y);

        // Jika belum ada history enemy
        if (!hasEnemyHistory)
        {
            // Isi  nilai ke riwayat terbaru dan menandakan bahwa history sudah tersedia
            recentEnemyX = evt.X;
            recentEnemyY = evt.Y;
            hasEnemyHistory = true;
        }

        // Memutar radar berdasarkan selisih sudut enemy dan sudut radar saat ini
        SetTurnRadarLeft(CalcDeltaAngle(directionToEnemy, RadarDirection));
        // Memutar body berdasarkan selisih sudut enemy dan sudut body saat ini
        SetTurnLeft(CalcDeltaAngle(directionToEnemy, Direction));

        // Selalu mendekat ke musuh dengan menyisakan jarak 50 unit pixel
        if (distanceToEnemy > 50)
            SetForward(distanceToEnemy - 50);

        // Menyesuaikan besar power secara dinamis berdasarkan jarak musuh antara 1 dan 3
        double firePower = (distanceToEnemy <= 100) ? 3.0 : 1.0;

        // Menghitung kecepatan peluru yang ditembakkan
        double bulletSpeed = CalcBulletSpeed(firePower);

        // Menghitung waktu yang dibutuhkan peluru untuk mencapai musuh
        double calcTravelTime = distanceToEnemy / bulletSpeed;

        // Mencari delta antara bot yang ter-scan dan riwayat terbaru bot musuh sebelumnya
        double deltaX = evt.X - recentEnemyX;
        double deltaY = evt.Y - recentEnemyY;

        // Akurasi (offset) prediksi pergerakan musuh
        double accurateFactor = 1.85;
        if (distanceToEnemy <= 100)
            accurateFactor = 0.5;
        else if (distanceToEnemy <= 250)
            accurateFactor = 1.0;
        else if (distanceToEnemy <= 500)
            accurateFactor = 1.45;

        /*
        if (accurateFactor != lastAccurateFactor)
        {
            Console.WriteLine($"[ANALISIS] Jarak: {Math.Round(distanceToEnemy, 1)} | Accurate Factor berubah ke: {accurateFactor}");
            lastAccurateFactor = accurateFactor;
        }
        */

        // Menghitung prediksi posisi musuh berikutnya dengan menambahkan posisi bot yang ter-scan dan hasil perhitungan perkiraan
        double predictedX = evt.X + (deltaX * calcTravelTime * accurateFactor);
        double predictedY = evt.Y + (deltaY * calcTravelTime * accurateFactor);

        // Selalu memperbarui riwayat terbaru dengan bot yang ter-scan
        recentEnemyX = evt.X;
        recentEnemyY = evt.Y;

        // Menghitung derajat yang diperlukan untuk memutar gun ke arah prediksi lokasi musuh
        double predictedAim = DirectionTo(predictedX, predictedY);
        double gunTurn = CalcDeltaAngle(predictedAim, GunDirection);
        SetTurnGunLeft(gunTurn);

        // Toleransi seberapa besar miss aim ke musuh
        double tolerance = (distanceToEnemy <= 100) ? 5 : 10;

        // Selama gun tidak panas dan masih sesuai nilai toleransi, lakukan penembakan
        if (Math.Abs(gunTurn) < tolerance && GunHeat == 0)
        {
            SetFire(firePower);
            // sumShots++;
        }

        // Mengeksekusi seluruh instruksi Set*()
        Go();
    }


    public override void OnConnected(ConnectedEvent connectedEvent)
    {
        // Menandakan bahwa bot berhasil masuk ke server
        Console.WriteLine("Bot Utama BinTank Lima berhasil masuk.");
    }
    public override void OnRoundStarted(RoundStartedEvent roundStatedEvent)
    {
        // Memastikan nilai dari ronde sebelumnya direset pada setiap ronde
        recentEnemyX = recentEnemyY = 0;
        hasEnemyHistory = false;
    }

    /*
    public override void OnHitByBullet(HitByBulletEvent bulletHitBotEvent) { }

    public override void OnBulletHit(BulletHitBotEvent bulletHitBotEvent)
    {
        sumHits++;
    }

    public override void OnBulletHitWall(BulletHitWallEvent bulletHitWallEvent)
    {
        Console.WriteLine($"[MISS] Menabrak Tembok! (Jarak target: {Math.Round(distanceToEnemy, 1)} | Factor: {lastAccurateFactor})");
        sumMiss++;
    }

    public override void OnBulletHitBullet(BulletHitBulletEvent bulletHitBulletEvent)
    {
        Console.WriteLine($"[MISS] Tabrakan di udara! (Jarak target: {Math.Round(distanceToEnemy, 1)} | Factor: {lastAccurateFactor})");
        sumMiss++;
    }

    public override void OnRoundEnded(RoundEndedEvent roundEndedEvent)
    {
        double accuracy = sumShots &gt; 0 ? (sumHits / sumShots) * 100 : 0;
        double sumBulletHitBullet = sumShots - sumHits - sumMiss; 

        Console.WriteLine($"Results");
        Console.WriteLine($"Shots            : {sumShots}");
        Console.WriteLine($"Hit              : {sumHits}");
        Console.WriteLine($"Miss             : {sumMiss}");
        Console.WriteLine($"BulletHitBullet  : {sumBulletHitBullet}");
        Console.WriteLine($"Percentage       : {Math.Round(accuracy, 2)}%");
    }
    */
}