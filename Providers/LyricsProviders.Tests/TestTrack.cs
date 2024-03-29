﻿using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;

namespace LyricsProviders.Tests
{
    public class TestTrack
    {
        public TrackInfo TrackInfo { get; set; }
        public ILyric Lyrics { get; set; }

        public static TestTrack SkilletHeroTrack { get; } = new TestTrack
        {
            TrackInfo = new TrackInfo
            {
                Artist = "skillet",
                Title = "hero",
                Album = "Hero"
            },
            Lyrics = new UnsyncedLyric(@"I'm just a step away
I'm just a breath away
Losin' my faith today
(Fallin' off the edge today)

I am just a man
Not superhuman
(I'm not superhuman)
Someone save me from the hate

It's just another war
Just another family torn
(Falling from my faith today)
Just a step from the edge
Just another day in the world we live

I need a hero to save me now
I need a hero (save me now)
I need a hero to save my life
A hero'll save me (just in time)

I've gotta fight today
To live another day
Speakin' my mind today
(My voice will be heard today)

I've gotta make a stand
But I am just a man
(I'm not superhuman)
My voice will be heard today

It's just another war
Just another family torn
(My voice will be heard today)
It's just another kill
The countdown begins to destroy ourselves

I need a hero to save me now
I need a hero (save me now)
I need a hero to save my life
A hero'll save me (just in time)

I need a hero to save my life
I need a hero just in time
Save me just in time
Save me just in time

Who's gonna fight for what's right
Who's gonna help us survive
We're in the fight of our lives
(And we're not ready to die)

Who's gonna fight for the weak
Who's gonna make 'em believe
I've got a hero (I've got a hero)
Livin' in me

I'm gonna fight for what's right
Today I'm speaking my mind
And if it kills me tonight
(I will be ready to die)

A hero's not afraid to give his life
A hero's gonna save me just in time

I need a hero to save me now
I need a hero (save me now)
I need a hero to save my life
A hero'll save me (just in time)

I need a hero
Who's gonna fight for what's right
Who's gonna help us survive

I need a hero
Who's gonna fight for the weak
Who's gonna make 'em believe
I need a hero
I need a hero

A hero's gonna save me just in time")
        };

        public static TestTrack AriaIceShardTrack { get; } = new TestTrack
        {
            TrackInfo = new TrackInfo
            {
                Artist = "ария",
                Title = "осколок льда",
                Album = "Химера"
            },
            Lyrics = new UnsyncedLyric(@"Осколок льда

Ночь унесла тяжелые тучи
Но дни горьким сумраком полны
Мы расстаемся - так будет лучше
Вдвоем нам не выбраться из тьмы

Я любил и ненавидел
Но теперь душа пуста
Все исчезло, не оставив и следа
И не знает боли в груди осколок льда

Я помню все, о чем мы мечтали
Но жизнь не для тех, кто любит сны
Мы слишком долго выход искали
Но шли бесконечно вдоль стены

Я любил и ненавидел
Но теперь душа пуста
Все исчезло, не оставив и следа
И не знает боли в груди осколок льда

Пусть каждый сам находит дорогу
Мой путь будет в сотню раз длинней
Но не виню ни черта, ни Бога
За все платить придется мне

Я любил и ненавидел
Но теперь душа пуста
Все исчезло, не оставив и следа
И не знает боли в груди осколок льда
И не знает боли в груди осколок льда")
        };

        public static TestTrack AriaImFreeTrack { get; } = new TestTrack
        {
            TrackInfo = new TrackInfo
            {
                Artist = "ария",
                Title = "я свободен",
                Album = "Путь наверх"
            },
            Lyrics = new UnsyncedLyric(@"Надо мною тишина,
Небо, полное дождя.
Дождь проходит сквозь меня,
Но боли больше нет.

Под холодный шепот звезд
Мы сожгли последний мост,
И все в бездну сорвалось.
Свободным стану я
От зла и от добра.
Моя душа была на лезвии ножа.

Я бы мог с тобою быть,
Я бы мог про все забыть,
Я бы мог тебя любить,
Но это лишь игра.

В шуме ветра за спиной
Я забуду голос твой,
И о той любви земной,
Что нас сжигала в прах,
И я сходил с ума.
В моей душе нет больше места для тебя.

Я свободен! Словно птица в небесах.
Я свободен! Я забыл, что значит страх.
Я свободен! С диким ветром наравне.
Я свободен! На яву, а не во сне.

Надо мною тишина,
Небо, полное огня.
Свет проходит сквозь меня,
И я свободен вновь.

Я свободен от любви,
От вражды и от молвы,
От предсказанной судьбы
И от земных оков,
От зла и от добра.
В моей душе нет больше места для тебя.

Я свободен! Словно птица в небесах.
Я свободен! Я забыл, что значит страх.
Я свободен! С диким ветром наравне.
Я свободен! Наяву, а не во сне.")
        };

        public override string ToString()
        {
            return TrackInfo.ToString();
        }
    }
}
