using ImposterGame.Models;

namespace ImposterGame.Services
{
    public class LocalizationService
    {
        public string Language { get; private set; } = "fa";
        public bool IsRtl => Language == "fa";
        public string Direction => IsRtl ? "rtl" : "ltr";

        public static string NormalizeLanguage(string? language) => language?.Trim().ToLowerInvariant() switch
        {
            "de" => "de",
            "en" => "en",
            _ => "fa"
        };

        public void SetLanguage(string? language)
        {
            Language = NormalizeLanguage(language);
        }

        public string T(string fa, string de, string en) => Language switch
        {
            "de" => de,
            "en" => en,
            _ => fa
        };

        public string AppTitle => T("بازی شیاد", "Imposter Spiel", "Imposter Game");
        public string NavHome => T("خانه", "Start", "Home");
        public string NavGame => T("بازی شیاد", "Spielen", "Play");
        public string NavAbout => T("درباره بازی", "Regeln", "Rules");
        public string NavSettings => T("تنظیمات", "Einstellungen", "Settings");
        public string NavSettingsTitle => T("⚙️ تنظیمات بازی", "⚙️ Spieleinstellungen", "⚙️ Game Settings");
        public string NavMenuLabel => T("منو", "Menü", "Menu");
        public string ToggleDarkModeLabel => T("تغییر حالت شب و روز", "Hell-/Dunkelmodus wechseln", "Toggle dark mode");

        // Home
        public string HeroSubtitle => T(
            "دورهمی، هیجان و بلوف! شیاد را بین دوستانتان پیدا کنید قبل از اینکه کلمه مخفی را حدس بزند.",
            "Party, Spannung & Bluff! Enttarnt den Imposter, bevor er das geheime Wort errät.",
            "Party, suspense & bluffing! Spot the imposter among your friends before they guess the secret word.");
        public string StatCategories(int count) => T($"📚 {count} دسته‌بندی", $"📚 {count} Kategorien", $"📚 {count} Categories");
        public string StatLevels => T("🎯 ۳ سطح سختی", "🎯 3 Schwierigkeitsstufen", "🎯 3 Difficulty Levels");
        public string StatWords(int count) => T($"✨ {count} کلمه", $"✨ {count} Wörter", $"✨ {count} Words");
        public string HomeStartButton => T("🎭 شروع بازی شیاد", "🎭 Spiel starten", "🎭 Play Imposter");
        public string HomeSettingsButton => T("⚙️ تنظیمات", "⚙️ Einstellungen", "⚙️ Settings");
        public string HomeRulesButton => T("📖 راهنما", "📖 Regeln", "📖 Rules");

        // Lobby
        public string ChangeButton => T("⚙️ تغییر", "⚙️ Ändern", "⚙️ Edit");
        public string PlayersTitle => T("👥 بازیکنان", "👥 Spieler", "👥 Players");
        public string PlayersCountBadge(int count) => T($"{count} نفر", $"{count} Spieler", $"{count} players");
        public string RemovePlayerAria(string name) => T($"حذف {name}", $"{name} entfernen", $"Remove {name}");
        public string EmptyPlayersLine1 => T("هنوز بازیکنی اضافه نشده است.", "Noch keine Spieler hinzugefügt.", "No players added yet.");
        public string EmptyPlayersLine2 => T("نام و آواتار اولین بازیکن را در بخش پایین انتخاب کنید 👇", "Gib unten Namen & Avatar des ersten Spielers ein 👇", "Enter the first player's name and avatar below 👇");
        public string StartGameWithCount(int count) => T($"🎮 شروع بازی ({count} بازیکن)", $"🎮 Spiel starten ({count} Spieler)", $"🎮 Start Game ({count} players)");
        public string NeedMorePlayers(int remaining, int current, int min) => T(
            $"برای شروع بازی {remaining} بازیکن دیگر اضافه کنید ({current} از {min})",
            $"Noch {remaining} Spieler hinzufügen zum Starten ({current} von {min})",
            $"Add {remaining} more player(s) to start ({current} of {min})");
        public string AddNewPlayerTitle => T("➕ افزودن بازیکن جدید", "➕ Neuen Spieler hinzufügen", "➕ Add New Player");
        public string PlayerNamePlaceholder => T("نام بازیکن را بنویسید...", "Spielername eingeben...", "Enter player name...");
        public string AddPlayerAria => T("افزودن بازیکن", "Spieler hinzufügen", "Add player");
        public string SelectAvatarLabel => T("انتخاب آواتار:", "Avatar wählen:", "Choose an avatar:");
        public string SelectAvatarAria => T("انتخاب آواتار", "Avatar wählen", "Select avatar");

        // Validation
        public string ValidationNameRequired => T("لطفاً نام بازیکن را وارد کنید.", "Bitte gib einen Spielernamen ein.", "Please enter a player name.");
        public string ValidationDuplicateName => T("این نام قبلاً اضافه شده است.", "Dieser Name wurde bereits hinzugefügt.", "This name has already been added.");
        public string ValidationAvatarRequired => T("لطفاً یک آواتار انتخاب کنید.", "Bitte wähle einen Avatar aus.", "Please choose an avatar.");
        public string ValidationMinPlayers(int min) => T($"برای شروع بازی حداقل {min} بازیکن لازم است.", $"Für den Start sind mindestens {min} Spieler nötig.", $"At least {min} players are required to start.");

        // Card Reveal (Hold to Reveal)
        public string PlayerProgress(int current, int total) => T($"بازیکن {current} از {total}", $"Spieler {current} von {total}", $"Player {current} of {total}");
        public string MysteryCardTitle => T("کارت مخفی آماده است", "Geheime Karte bereit", "Secret card is ready");
        public string MysteryCardSeenTitle => T("کارت مخفی شد", "Karte wieder verdeckt", "Card hidden again");
        public string MysteryCardHint(string name) => T(
            $"مطمئن شوید فقط {name} صفحه را می‌بیند",
            $"Achte darauf, dass nur {name} aufs Display schaut",
            $"Make sure only {name} can see the screen");
        public string HoldToRevealButton => T("👆 نگه دارید تا کارت نمایش داده شود", "👆 Gedrückt halten zum Ansehen", "👆 Press & hold to view card");
        public string HoldingButton => T("👀 در حال نمایش — رها کنید تا مخفی شود", "👀 Sichtbar — loslassen zum Verdecken", "👀 Showing — release to hide");
        public string HoldAgainHint => T("برای دیدن دوباره می‌توانید دکمه را نگه دارید", "Zum erneuten Ansehen einfach wieder gedrückt halten", "Hold the button again if you need another peek");
        public string ImposterCardTitle => T("🕵️ شما شیاد هستید!", "🕵️ Du bist der Imposter!", "🕵️ You are the Imposter!");
        public string ImposterCategoryHint(string icon, string categoryName) => T(
            $"💡 دسته‌بندی: {icon} {categoryName}",
            $"💡 Kategorie: {icon} {categoryName}",
            $"💡 Category: {icon} {categoryName}");
        public string ImposterTipMulti(int count) => T(
            $"در این دور {count} شیاد در بازی هستند — طبیعی رفتار کنید و کلمه را حدس بزنید!",
            $"In dieser Runde gibt es {count} Imposter — verhalte dich unauffällig und errate das Wort!",
            $"There are {count} imposters this round — blend in and guess the secret word!");
        public string ImposterTipSingle => T(
            "طبیعی رفتار کنید و از روی سرنخ‌ها کلمه اصلی را حدس بزنید!",
            "Verhalte dich unauffällig und errate das geheime Wort aus den Hinweisen!",
            "Blend in naturally and figure out the secret word from the clues!");
        public string NextPlayerButton => T("➡️ نفر بعدی", "➡️ Nächster Spieler", "➡️ Next Player");
        public string StartTimerButton => T("🎮 شروع زمان بازی", "🎮 Timer starten", "🎮 Start Game Timer");
        public string BackToLobbyButton => T("بازگشت به لیست بازیکنان", "Zurück zur Spielerliste", "Back to player list");

        // Timer
        public string TimerPageTitle => T("زمان بازی", "Spiel-Timer", "Game Timer");
        public string StartsFirst(string name) => T($"🎤 {name} شروع می‌کند", $"🎤 {name} beginnt", $"🎤 {name} goes first");
        public string MultiImposterBadge(int count) => T($"🕵️ {count} شیاد در این دور حضور دارند", $"🕵️ {count} Imposter in dieser Runde", $"🕵️ {count} imposters this round");
        public string TimerStatusRunning => T("در حال گفتگو", "Diskussion läuft", "Discussion active");
        public string TimerStatusPaused => T("متوقف شده", "Pausiert", "Paused");
        public string TimerStatusDone => T("پایان زمان", "Zeit abgelaufen", "Time's up");
        public string TimeUpBanner => T("⏰ زمان تمام شد! حالا وقت رای‌گیری است", "⏰ Die Zeit ist um! Jetzt wird abgestimmt", "⏰ Time's up! Time to vote");
        public string PauseButton => T("⏸️ توقف", "⏸️ Pause", "⏸️ Pause");
        public string ResumeButton => T("▶️ ادامه", "▶️ Weiter", "▶️ Resume");
        public string RevealResultShort => T("🎭 نمایش نتیجه", "🎭 Auflösen", "🎭 Reveal");
        public string RevealResultFull(int imposterCount) => imposterCount > 1
            ? T("🎭 نمایش شیادها و کلمه", "🎭 Imposter & Wort zeigen", "🎭 Reveal Imposters & Word")
            : T("🎭 نمایش شیاد و کلمه", "🎭 Imposter & Wort zeigen", "🎭 Reveal Imposter & Word");
        public string ResultImposterHeader(int imposterCount) => imposterCount > 1
            ? T("🕵️ شیادهای این دور", "🕵️ Die Imposter dieser Runde", "🕵️ Imposters This Round")
            : T("🕵️ شیاد این دور", "🕵️ Der Imposter dieser Runde", "🕵️ Imposter This Round");
        public string ResultSecretWord(string word) => T($"کلمه مخفی: {word}", $"Geheimes Wort: {word}", $"Secret word: {word}");
        public string NewRoundButton => T("🔁 دور جدید بازی", "🔁 Neue Runde", "🔁 New Round");

        // Confirm Dialogs
        public string ConfirmExitTitle => T("⚠️ توقف بازی", "⚠️ Spiel verlassen", "⚠️ Leave Game");
        public string ConfirmExitMessage => T(
            "بازی در حال اجراست. آیا مطمئن هستید که می‌خواهید از بازی خارج شوید؟",
            "Das Spiel läuft gerade. Möchtest du die Runde wirklich beenden?",
            "A game is currently in progress. Are you sure you want to leave?");
        public string ConfirmExitOk => T("بله، خروج از بازی", "Ja, verlassen", "Yes, leave");
        public string ConfirmContinue => T("ادامه بازی", "Weiterspielen", "Keep playing");
        public string ConfirmRestartTitle => T("⚠️ پایان بازی", "⚠️ Runde beenden", "⚠️ End Round");
        public string ConfirmRestartMessage => T(
            "بازی هنوز در حال اجراست. آیا مطمئن هستید که می‌خواهید بازی را متوقف کنید؟",
            "Die Runde läuft noch. Möchtest du wirklich eine neue Runde starten?",
            "This round is still active. Are you sure you want to start a new round?");
        public string ConfirmRestartOk => T("بله، دور جدید", "Ja, neue Runde", "Yes, new round");
        public string ConfirmRevealTitle => T("🎭 نمایش نتیجه", "🎭 Ergebnis zeigen", "🎭 Reveal Result");
        public string ConfirmRevealMessage => T(
            "زمان بازی هنوز تمام نشده است. آیا می‌خواهید شیاد و کلمه نمایش داده شود؟",
            "Die Zeit läuft noch. Sollen Imposter und geheimes Wort jetzt schon aufgedeckt werden?",
            "Time hasn't run out yet. Reveal the imposter and secret word now?");
        public string ConfirmRevealOk => T("بله، نمایش بده", "Ja, aufdecken", "Yes, reveal");

        // Settings Dialog
        public string SettingsLanguageTitle => T("🌍 زبان بازی", "🌍 Sprache", "🌍 Language");
        public string SettingsAppearanceTitle => T("🌙 حالت شب (Dark Mode)", "🌙 Dunkelmodus (Dark Mode)", "🌙 Dark Mode");
        public string SettingsAppearanceHint => T("نمای تیره و آرام برای بازی در شب", "Augenfreundlich für gemütliche Abende", "Cozy low-light theme for evening play");
        public string SettingsSoundTitle => T("🔊 صدا و لرزش", "🔊 Sound & Vibration", "🔊 Sound & Haptics");
        public string SettingsSoundHint => T("افکت صوتی کارت، تایمر ۱۰ ثانیه آخر و ویبره", "Karten-Sound, 10-Sek.-Ticken & Vibration", "Card chime, final 10s ticks & vibration");
        public string SettingsDifficultyTitle => T("🎯 سطح بازی", "🎯 Schwierigkeitsstufe", "🎯 Difficulty Level");
        public string DifficultyLabel(WordDifficulty difficulty) => difficulty switch
        {
            WordDifficulty.Easy => T("🟢 آسان", "🟢 Leicht", "🟢 Easy"),
            WordDifficulty.Medium => T("🟡 متوسط", "🟡 Mittel", "🟡 Medium"),
            WordDifficulty.Hard => T("🔴 سخت", "🔴 Schwer", "🔴 Hard"),
            _ => T("🟡 متوسط", "🟡 Mittel", "🟡 Medium")
        };
        public string DifficultyDescription(WordDifficulty difficulty) => difficulty switch
        {
            WordDifficulty.Easy => T("فقط کلمات ساده و روزمره — مناسب برای همه", "Nur einfache Alltagswörter — ideal für alle", "Simple everyday words only — great for everyone"),
            WordDifficulty.Medium => T("فقط کلمات متعادل و متنوع — چالش استاندارد", "Ausgewogene, abwechslungsreiche Wörter", "Balanced, varied words — classic challenge"),
            WordDifficulty.Hard => T("فقط کلمات بسیار سخت، تخصصی و مفهومی — برای حرفه‌ای‌ها!", "Nur sehr knifflige & abstrakte Wörter — für Profis!", "Tricky, niche & abstract words only — for pros!"),
            _ => string.Empty
        };
        public string SettingsCategoriesTitle => T("📚 دسته‌بندی کلمات", "📚 Wort-Kategorien", "📚 Word Categories");
        public string WordsCountPill(int count) => T($"{count} کلمه", $"{count} Wörter", $"{count} words");
        public string AllCategoriesChip => T("✨ همه دسته‌ها", "✨ Alle Kategorien", "✨ All Categories");
        public string SettingsImpostersTitle => T("🕵️ تعداد شیادها", "🕵️ Anzahl Imposter", "🕵️ Imposter Count");
        public string ImposterOption(int count) => T($"{count} شیاد", $"{count} Imposter", count == 1 ? "1 Imposter" : $"{count} Imposters");
        public string TwoImpostersHint(int minPlayers) => T(
            $"برای انتخاب ۲ شیاد حداقل {minPlayers} بازیکن لازم است.",
            $"Für 2 Imposter werden mindestens {minPlayers} Spieler benötigt.",
            $"At least {minPlayers} players are required for 2 imposters.");
        public string SettingsAutoTimeTitle => T("⏱️ زمان خودکار", "⏱️ Automatische Zeit", "⏱️ Auto Timer");
        public string SettingsAutoTimeHint => T("۱ دقیقه به ازای هر بازیکن", "1 Minute pro Spieler", "1 minute per player");
        public string MinutesLabel(int minutes) => T($"{minutes} دقیقه", $"{minutes} Min.", $"{minutes} min");
        public string SettingsHintTitle => T("💡 راهنمایی برای شیاد", "💡 Kategorie-Hinweis", "💡 Imposter Hint");
        public string SettingsHintDesc => T("شیاد فقط نام دسته‌بندی کلمه را می‌بیند", "Imposter sieht nur die Kategorie des Wortes", "Imposter sees only the word's category");
        public string InstallAppButton => T("📲 نصب اپلیکیشن روی گوشی", "📲 App auf Startbildschirm installieren", "📲 Install App on Home Screen");
        public string InstallAppShort => T("نصب اپ", "Installieren", "Install");
        public string IosInstallHint => T(
            "📱 نصب روی آیفون/آیپد: دکمه Share (⎋) را در سافاری بزنید و «Add to Home Screen» را انتخاب کنید.",
            "📱 Installation auf iPhone/iPad: In Safari auf Teilen (⎋) tippen und „Zum Home-Bildschirm“ wählen.",
            "📱 Install on iPhone/iPad: Tap Share (⎋) in Safari and select “Add to Home Screen”.");
        public string BrowserInstallHint => T(
            "📲 برای نصب به عنوان اپلیکیشن: در کروم یا اج از منوی مرورگر گزینه «Install app / افزودن به صفحه اصلی» را انتخاب کنید.",
            "📲 Als App installieren: In Chrome/Edge einfach auf das Installieren-Symbol (⊕) in der Adressleiste oder im Menü auf „App installieren“ tippen.",
            "📲 Install as an app: In Chrome/Edge, click the install icon (⊕) in the address bar or choose “Install app” from the browser menu.");
        public string PrivacyPolicyLink => T("🔒 حریم خصوصی", "🔒 Datenschutz", "🔒 Privacy Policy");
        public string SaveAndCloseButton => T("✓ تأیید و بستن", "✓ Fertig & Schließen", "✓ Save & Close");

        public string BuildSettingsSummary(GameSettings settings, int effectiveImposters, int wordPoolCount)
        {
            var levelPart = DifficultyLabel(settings.Difficulty);
            var categoryPart = settings.SelectedCategoryIds.Count == 0
                ? T("همه دسته‌ها", "Alle Kategorien", "All categories")
                : T($"{settings.SelectedCategoryIds.Count} دسته", $"{settings.SelectedCategoryIds.Count} Kategorien", $"{settings.SelectedCategoryIds.Count} categories");
            var imposterPart = ImposterOption(effectiveImposters);
            var timePart = settings.AutoTimePerPlayer
                ? T("زمان خودکار", "Auto-Zeit", "Auto time")
                : MinutesLabel(settings.RoundDurationMinutes);
            var hintPart = settings.ShowCategoryHintToImposter
                ? T(" • با راهنمایی", " • Mit Hinweis", " • With hint")
                : string.Empty;

            return $"{levelPart} • {categoryPart} ({wordPoolCount}) • {imposterPart} • {timePart}{hintPart}";
        }

        // NotFound
        public string NotFoundTitle => T("صفحه مورد نظر پیدا نشد", "Seite nicht gefunden", "Page Not Found");
        public string NotFoundMessage => T("آدرسی که وارد کرده‌اید وجود ندارد.", "Die gesuchte Adresse existiert leider nicht.", "The address you entered does not exist.");
        public string BackHomeButton => T("🏠 بازگشت به خانه", "🏠 Zur Startseite", "🏠 Back to Home");
    }
}
