using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class AllMight : Bot
{
    // Unutk menyimpan target id
    int targetId = -1;
    // Menyimpan jarak musuh yang menjadi target
    double lockedDistance = double.MaxValue;
    // Mencatat pada putaran (turn) ke berapa bot terakhir kali melihat target.
    int lastScanTurn = 0;
    // Menyimpan koordinat X dan Y musuh di masa lalu
    double prevX = 0, prevY = 0;
    // Menentukan arah orbit
    int orbitDirection = 1;
    // Mencatat kapan kapan boleh menembak lagi
    int readyToFireTurn = 0;
    // Mencatat kwaktu terakhir kalo bot berbalik
    int lastReverseTurn = 0;
    // Menyimpan batas waktu untuk menghindar
    int evasionEndTurn = 0;
    // Membuat angka acak untuk membuat gerakan sulit dipredi
    Random rnd = new Random();

    static void Main(string[] args) => new AllMight().Start();
    AllMight() : base(BotInfo.FromFile("AllMight.json")) { }

    public override void Run()
    {
        // Memisahkan pergerakan radar, meriam, dan badan tank menjadi modular
        AdjustRadarForGunTurn = AdjustGunForBodyTurn = true;

        // Mengatur warna bot
        BodyColor = Color.RoyalBlue; TurretColor = GunColor = BulletColor = Color.Gold; RadarColor = Color.White;

        // Memutar radar, maju, dan berbelok
        SetTurnRadarLeft(360); SetForward(100); SetTurnLeft(90); Go();

        while (IsRunning)
        {
            // Memulai pencarian ulang jika target hilang melebihi batas waktu
            if (targetId == -1 || TurnNumber - lastScanTurn > ((EnemyCount > 1) ? 15 : 3))
            {
                // Menghapus data target lama.
                ResetTarget();
                // Memutar radar, bergerak mengorbit
                SetTurnRadarLeft(360); SetTurnLeft(20 * orbitDirection); SetForward(150);
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Menghitung jarak musuh yang terscan
        double dist = DistanceTo(e.X, e.Y);

        // Hanya memproses musuh ini jika ia target lama, kita sedang tidak ada target, atau musuh ini jauh lebih dekat.
        if (targetId == e.ScannedBotId || targetId == -1 || dist < lockedDistance - 120)
        {
            // Jika ini musuh baru, simpan koordinatnya dan hapus hitungan cooldown senjata sebelumnya.
            if (targetId != e.ScannedBotId) { prevX = e.X; prevY = e.Y; readyToFireTurn = 0; }

            // Perbarui data target saat ini.
            targetId = e.ScannedBotId; lockedDistance = dist; lastScanTurn = TurnNumber;

            // Menghitung arah musuh untuk radar agar memastikan tidak diam saja
            double radarTurn = CalcDeltaAngle(DirectionTo(e.X, e.Y), RadarDirection);
            // Menentukan lebar sapuan radar, 15 derajat untuk keramaian, 2 derajat untuk duel
            int radarSpread = (EnemyCount > 1) ? 15 : 2;
            // Mengunci radar ke arah musuh dengan sedikit dilebarkan
            SetTurnRadarLeft(radarTurn + ((radarTurn >= 0) ? radarSpread : -radarSpread));

            // Menghitung selisih jarak musuh (Prediksi gerakan)
            double dX = (prevX > 0) ? (e.X - prevX) : 0, dY = (prevY > 0) ? (e.Y - prevY) : 0;
            // Menentukan direksi agar melakukan manuver tikungan tajam (10 derajat)
            double angleToIntercept = DirectionTo(e.X + (dX * 10), e.Y + (dY * 10));

            // Mendeteksi apakah musuh sedang diam tak bergerak
            bool isStationary = (Math.Abs(dX) < 0.1 && Math.Abs(dY) < 0.1);

            // Pada awal permainan, evaluasi titik tengah arena tiap 15 turn
            if (EnemyCount > 2 && TurnNumber % 15 == 0)
            {
                // Menentukan sudut ke tengah arena.
                double angleToCenter = DirectionTo(ArenaWidth / 2.0, ArenaHeight / 2.0);
                // Memilih arah untuk menjauhi atau membelakangi area tengah
                orbitDirection = (Math.Abs(CalcDeltaAngle(angleToIntercept + 90, angleToCenter)) > Math.Abs(CalcDeltaAngle(angleToIntercept - 90, angleToCenter))) ? 1 : -1;
            }

            // Deklarasi variabel arah gerak
            double moveAngle;
            // Menambahkan gerakan acak (-10 dan 10), tapi matikan jika musuh diam
            int wobble = isStationary ? 0 : rnd.Next(-10, 11);

            // Mempertimbangkan untuk menabrak jika musuh kurang dari 3 atau jarak sangat dekat, dan energi musuh di bawah 60
            bool shouldRam = (EnemyCount < 3 || dist <= 100) && e.Energy < 60;

            // Jika syarat menabrak terpenuhi, prioritaskan untuk menabrak musuh
            if (shouldRam)
            {
                // Tabrak lurus tanpa ampun ke arah prediksi musuh
                moveAngle = angleToIntercept;
            }
            // Jika musuh banyak, bermain pasif dan hindari pertempuran langsung
            else if (EnemyCount > 2)
            {
                // Menjauh jika sedang mode koservatif
                if (TurnNumber < evasionEndTurn) moveAngle = angleToIntercept + ((110 + wobble) * orbitDirection);
                // Mendekat sedeikit ajah jika musuh terlalu jauh
                else if (dist > 350) moveAngle = angleToIntercept + ((40 + wobble) * orbitDirection);
                // Mengorbit dijarak aman
                else if (dist > 200) moveAngle = angleToIntercept + ((80 + wobble) * orbitDirection);
                // Menjauh jika terlalu dekat dengan musuh
                else moveAngle = angleToIntercept + ((130 + wobble) * orbitDirection);
            }
            // Musuh tersisa 2, lebih agresif
            else if (EnemyCount == 2)
            {
                // Jika terkena tembakan, mundur 100 derajat dengan variasi gerakan
                if (TurnNumber < evasionEndTurn) moveAngle = angleToIntercept + ((100 + wobble) * orbitDirection);
                // Mendekat dengan elevasi 35 derajat
                else if (dist > 250) moveAngle = angleToIntercept + ((35 + wobble) * orbitDirection);
                // JIka diatas 150, maju dan mengorbit 75 derajat
                else if (dist > 150) moveAngle = angleToIntercept + ((75 + wobble) * orbitDirection);
                // Menjauh jika ada musuh didekat dan musuh yang hidup masih banyak
                else moveAngle = angleToIntercept + ((110 + wobble) * orbitDirection);
            }
            // Musuh tersisa 1, menjadi agresif
            else
            {
                // Jika terkena tembakan, tegak mundur 90 derajat
                if (TurnNumber < evasionEndTurn) moveAngle = angleToIntercept + ((90 + wobble) * orbitDirection);
                // Mendekat dengan elevasi 30 derajat
                else if (dist > 150) moveAngle = angleToIntercept + ((30 + wobble) * orbitDirection);
                // Maju dengan sudut 45 derajat
                else moveAngle = angleToIntercept + ((45 + wobble) * orbitDirection);

                // Peluang untuk memicu gerakan random
                if (rnd.Next(0, 100) < 3 && TurnNumber - lastReverseTurn > 15) { orbitDirection *= -1; lastReverseTurn = TurnNumber; }
            }

            // Memproyeksikan tujuan bot agar tidak menabrak tembok
            double nextX = Math.Max(60, Math.Min(ArenaWidth - 60, X + (Math.Sin(moveAngle * Math.PI / 180) * 150)));
            double nextY = Math.Max(60, Math.Min(ArenaHeight - 60, Y + (Math.Cos(moveAngle * Math.PI / 180) * 150)));

            // Cek apakah bot sedang terjebak di pojokan
            bool isCornerTrapped = DistanceTo(nextX, nextY) < 5;

            // Mencegah berhenti dipojokan, alihkan ke tengah
            moveAngle = isCornerTrapped ? DirectionTo(ArenaWidth / 2.0, ArenaHeight / 2.0) : DirectionTo(nextX, nextY);

            // Menghitung Manuver bot
            double turnAngle = CalcDeltaAngle(moveAngle, Direction);
            // Variabel gerak, 1 = maju
            int gasDirection = 1;
            // Jika harus bermanuver lebih dari 90 derajat, buat bot mundur saja
            if (Math.Abs(turnAngle) > 90) { turnAngle = CalcDeltaAngle(moveAngle + 180, Direction); gasDirection = -1; }

            // Berhenti melaju jika terjebak tembok dan musuh sudah dekat (Rem Algojo)
            if (isCornerTrapped && dist < 150) gasDirection = 0;

            // Bermanuver dan maju dengan kecepatan penuh
            SetTurnLeft(turnAngle); SetForward(150 * gasDirection); MaxSpeed = 8;

            // Mengatur kekuatn tembak berdasarkan jarak
            double firePower = (dist <= 150) ? 3.0 : (dist <= 300) ? 2.0 : (dist <= 450) ? 1.0 : 0;
            // Jika energy rendah dan musuh banyak/awal game, bermain aman (Tidak menembak)
            if (EnemyCount > 2 && Energy < 40 && dist > 200) firePower = 0;
            // Mencegah mati karena menembak saat energi hampir habis
            if (Energy < 10) firePower = Math.Min(firePower, 0.1);

            // Jika cooldown tembak selesai dan musuh dekat bisa menembak
            if (firePower > 0 && (TurnNumber >= readyToFireTurn || dist <= 120))
            {
                double gunTurn;

                // Jika musuh sangat dekat atau diam, tembak langsung ke arahnya
                if (dist <= 60 || isStationary)
                {
                    gunTurn = CalcDeltaAngle(DirectionTo(e.X, e.Y), GunDirection);
                }
                else
                {
                    // Titik tebakan awal peluru
                    double pX = e.X, pY = e.Y;
                    // Menghitung kecepatan peluru kita
                    double bSpeed = CalcBulletSpeed(firePower);
                    // Mengekstrak kecepatan gerak X dan Y musuh
                    double eHeading = e.Direction * Math.PI / 180.0;
                    double vX = Math.Sin(eHeading) * e.Speed;
                    double vY = Math.Cos(eHeading) * e.Speed;

                    // Mengulang perhitungan 5 kali untuk menebak titik temu paling akurat (Sniper Orion)
                    for (int i = 0; i < 5; i++)
                    {
                        // Menghitung waktu peluru ke arah prediksi
                        double time = DistanceTo(pX, pY) / bSpeed;
                        // Memastikan tebakan prediksi tidak melebihi tembok
                        pX = Math.Max(18, Math.Min(ArenaWidth - 18, e.X + vX * time));
                        pY = Math.Max(18, Math.Min(ArenaHeight - 18, e.Y + vY * time));
                    }

                    // Menghitung sudut meriam menuju titik temu
                    gunTurn = CalcDeltaAngle(DirectionTo(pX, pY), GunDirection);
                }

                // Arahkan meriam ke sudut yang dituju
                SetTurnGunLeft(gunTurn);
                // Tembak jika mengarah ke musuh dan tidak panas
                if (Math.Abs(gunTurn) < ((dist <= 60) ? 30 : 10) && GunHeat == 0) SetFire(firePower);
            }
            // Rekam jejak musuh untuk ronde berikutnya
            prevX = e.X; prevY = e.Y;
        }
    }

    // Jika peluru kena, maka tembak lagi
    public override void OnBulletHit(BulletHitBotEvent e) => readyToFireTurn = 0;

    // Jika peluru meleset ke tembok, berhenti menembak 5 putaran dan lupakan target agar radar mencari musuh yang posisinya terbuka
    public override void OnBulletHitWall(BulletHitWallEvent e) { readyToFireTurn = TurnNumber + 5; ResetTarget(); }

    // Jika peluru menabrak peluru lain, berhenti menembak 5 putaran dan lupakan target untuk menghindari baku tembak macet
    public override void OnBulletHitBullet(BulletHitBulletEvent e) { readyToFireTurn = TurnNumber + 5; ResetTarget(); }

    // Jika tertembak, melakukan manuver
    public override void OnHitByBullet(HitByBulletEvent e) => HandleEmergency(true);

    // Jika tertabrak, menghindar tapi target tetap sama
    public override void OnHitBot(HitBotEvent e)
    {
        // Jika kita memang sengaja menabrak untuk menghabisi musuh (energi di bawah 60), abaikan refleks mundur
        if (e.Energy < 60) return;

        // Menghindar instan jika tertabrak tidak sengaja (oleh musuh berenergi tinggi)
        HandleEmergency(false);
    }

    // Jika target sekarang hancur, reset target
    public override void OnBotDeath(BotDeathEvent e) { if (e.VictimId == targetId) ResetTarget(); }

    // Jika menabrak tmebok, reset target, amankan posisi
    public override void OnHitWall(HitWallEvent e) { if (TurnNumber - lastReverseTurn > 10) { orbitDirection *= -1; lastReverseTurn = TurnNumber; } ResetTarget(); }

    // Menghindar instan
    void HandleEmergency(bool resetTarget)
    {
        // Menetapkan menghindar
        evasionEndTurn = TurnNumber + 20;
        // Balikkan gerakkan agar menghindari serangann
        if (TurnNumber - lastReverseTurn > 15) { orbitDirection *= -1; lastReverseTurn = TurnNumber; }
        // Lupakan target lama
        if (resetTarget) ResetTarget();
    }

    // Reset otomatis
    void ResetTarget() { targetId = -1; lockedDistance = double.MaxValue; }
}