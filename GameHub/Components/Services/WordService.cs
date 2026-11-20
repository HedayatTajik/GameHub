namespace ImposterGame.Services
{
    public class WordService
    {
        private readonly Queue<string> _remainingWords = new();
        private string? _lastWord;

        // Words with optional icons added
        public List<string> Words { get; set; } = new()
        {
            "🍎 سیب", "🍌 موز", "🏠 خانه", "🚗 ماشین", "🌳 درخت",
            "🐶 سگ", "🐦 پرنده", "📚 کتاب", "🪑 میز",
            "🏫 مدرسه", "🛣️ خیابان", "⛰️ کوه", "🏞️ رودخانه",
            "🌊 دریا", "☁️ آسمان", "☀️ خورشید", "🌙 ماه", "⭐ ستاره",
            "💡 چراغ", "🚪 در", "📄 کاغذ", "✏️ قلم",
            "📓 دفتر", "📞 تلفن", "💻 کامپیوتر", "💻 لپ‌تاپ", "📱 موبایل",
            "⏰ ساعت", "👟 کفش", "👕 لباس", "🎩 کلاه", "🧤 دستکش",
            "🕶️ عینک", "⚽ توپ", "🚲 دوچرخه",
            "🏍️ موتورسیکلت", "🚌 اتوبوس", "🚆 قطار", "✈️ هواپیما", "🚢 کشتی",
            "🏞️ دریاچه", "🌷 باغ", "🌹 گل", "🌿 گیاه",
            "🥒 خیار", "🍊 پرتقال", "🍋 لیمو",
            "🍇 انگور", "🌰 بادام",
            "🥛 شیر", "🧀 پنیر", "🥣 ماست", "🍞 نان", "🍚 برنج",
            "🥩 گوشت", "🍗 مرغ", "🐟 ماهی", "🥚 تخم‌مرغ", "☕ قهوه",
            "🍵 چای", "💧 آب", "🥤 نوشابه", "🍫 شکلات", "🍰 کیک",
            "🍦 بستنی", "🍯 عسل", "🕯️ شمع",
            "🔪 چاقو", "🥄 قاشق",
            "📺 تلویزیون", "📻 رادیو", "📱 تبلت", "🧼 صابون", "🧴 شامپو",
            "🪞 آینه", "🧹 جارو",
            "🪟 پرده", "📚 کتابخانه", "🛏️ تخت", "🛏️ بالشت",
            "🛏️ پتو", "🧺 لباسشویی", "🧊 یخچال",
        };

        /// <summary>
        /// Returns a random word without repeating until all words have been used.
        /// </summary>
        public string GetRandomWord()
        {
            if (Words.Count == 0)
            {
                return string.Empty;
            }

            if (_remainingWords.Count == 0)
            {
                RefillShuffledQueue();
            }

            var nextWord = _remainingWords.Dequeue();
            _lastWord = nextWord;
            return nextWord;
        }

        private void RefillShuffledQueue()
        {
            var shuffled = Words.ToList();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            // Avoid repeating the immediately previous word across reshuffles
            if (shuffled.Count > 1 && shuffled[0] == _lastWord)
            {
                int swapIndex = Random.Shared.Next(1, shuffled.Count);
                (shuffled[0], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[0]);
            }

            foreach (var word in shuffled)
            {
                _remainingWords.Enqueue(word);
            }
        }
    }
}
