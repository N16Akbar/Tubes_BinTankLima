using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Tester : Bot
{
    // Arah pergerakan: 1 untuk maju, -1 untuk mundur
    int moveDirection = 1; 

    static void Main(string[] args)
    {
        new Tester().Start();
    }

    // Pastikan Anda juga memiliki file bernama "Tester.json" di folder yang sama
    Tester() : base(BotInfo.FromFile("Tester.json")) { }

    public override void Run()
    {
        BodyColor = Color.DarkRed;
        TurretColor = Color.Black;
        RadarColor = Color.Red;

        // Baris pengaturan IsAdjustGun... dan IsAdjustRadar... dihapus untuk
        // menghindari Error CS0103 (menyesuaikan versi API C# Anda).

        while (IsRunning)
        {
            // Jika tidak sedang melihat musuh, putar radar untuk mencari
            TurnRadarRight(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // ----------------------------------------------------
        // 1. RADAR LOCK (Mengunci Radar ke Musuh)
        // ----------------------------------------------------
        double radarTurn = evt.Direction - RadarDirection;
        // Normalisasi sudut agar mengambil rute terpendek
        while (radarTurn > 180) radarTurn -= 360;
        while (radarTurn < -180) radarTurn += 360;
        
        // Tambahkan ekstra sedikit putaran agar radar menyapu melewati musuh
        radarTurn += (radarTurn >= 0 ? 5 : -5);
        TurnRadarRight(radarTurn);


        // ----------------------------------------------------
        // 2. STRAFING MOVEMENT (Gerakan Menyamping / Menghindar)
        // ----------------------------------------------------
        double bearing = evt.Direction - Direction;
        while (bearing > 180) bearing -= 360;
        while (bearing < -180) bearing += 360;

        // Berbelok 90 derajat dari arah musuh (bergerak menyamping)
        TurnRight(bearing + 90);
        
        // Bergerak maju atau mundur tergantung nilai moveDirection
        Forward(150 * moveDirection);


        // ----------------------------------------------------
        // 3. PREDICTIVE AIMING (Menembak ke Masa Depan)
        // ----------------------------------------------------
        double firePower = 2.0;
        // Rumus kecepatan peluru di Robocode: 20 - (3 * firepower)
        double bulletSpeed = 20 - (3 * firePower);
        double distance = DistanceTo(evt.X, evt.Y);
        
        // Perkiraan waktu peluru sampai ke target
        double time = distance / bulletSpeed;

        // Prediksi koordinat X dan Y musuh di masa depan menggunakan Trigonometri
        double predictedX = evt.X + Math.Sin(evt.Direction * Math.PI / 180) * evt.Speed * time;
        double predictedY = evt.Y + Math.Cos(evt.Direction * Math.PI / 180) * evt.Speed * time;

        // Menghitung arah meriam ke koordinat prediksi tersebut
        double aimDirection = DirectionTo(predictedX, predictedY);
        double gunTurn = aimDirection - GunDirection;

        while (gunTurn > 180) gunTurn -= 360;
        while (gunTurn < -180) gunTurn += 360;

        TurnGunRight(gunTurn);

        // Hanya tembak jika meriam sudah selesai mendingin dan bidikan sudah akurat
        if (GunHeat == 0 && Math.Abs(gunTurn) < 5)
        {
            Fire(firePower);
        }
    }

    // Jika bot menabrak dinding, balikkan arahnya (jika tadinya maju, jadi mundur)
    public override void OnHitWall(HitWallEvent evt)
    {
        moveDirection *= -1; 
    }
}