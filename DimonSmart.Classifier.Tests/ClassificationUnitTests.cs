using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Xunit.Abstractions;

namespace DimonSmart.Classifier.Tests
{
    public class ClassificationUnitTests
    {
        private readonly ITestOutputHelper _output;

        public ClassificationUnitTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test1Async()
        {
            var movieData = new List<Movie>()
            {
                new Movie
                {
                    Key=0,
                    Title="The Snow Queen",
                    Description="An animated film based on Hans Christian Andersen's fairy tale about Gerda's journey to rescue her friend Kai from the Snow Queen."
                },
                new Movie
                {
                    Key=1,
                    Title="Hedgehog in the Fog",
                    Description="A short animated film about a hedgehog who gets lost in a mysterious fog while visiting his friend."
                },
                new Movie
                {
                    Key=2,
                    Title="Three from Prostokvashino",
                    Description="A boy named Uncle Fyodor runs away from home and lives with a talking cat and dog in the village of Prostokvashino."
                },
                new Movie
                {
                    Key=3,
                    Title="Well, Just You Wait!",
                    Description="An iconic animated series featuring the comedic rivalry between a wolf and a hare."
                },
                new Movie
                {
                    Key=4,
                    Title="Cheburashka",
                    Description="An animated series about a friendly creature named Cheburashka and his friend Gena the Crocodile."
                },
                new Movie
                {
                    Key=5,
                    Title="The Bremen Town Musicians",
                    Description="An animated musical inspired by the Grimm Brothers' fairy tale about animal musicians."
                },
                new Movie
                {
                    Key=6,
                    Title="Masha and the Bear",
                    Description="A popular animated series about a mischievous girl named Masha and her friend, a retired circus bear."
                },
                new Movie
                {
                    Key=7,
                    Title="The Scarlet Flower",
                    Description="An animated adaptation of the Russian folk version of 'Beauty and the Beast.'"
                },
                new Movie
                {
                    Key=8,
                    Title="Vovka in the Far Far Away Kingdom",
                    Description="A humorous animated short about a lazy boy named Vovka who wishes to live in a fairy-tale land."
                },
                new Movie
                {
                    Key=9,
                    Title="Little Golden Calf",
                    Description="A comedy film about con artist Ostap Bender's quest to swindle a secret millionaire."
                },
                new Movie
                {
                    Key=10,
                    Title="The Irony of Fate",
                    Description="A romantic comedy where a man's accidental trip to Leningrad leads to an unexpected romance."
                },
                new Movie
                {
                    Key=11,
                    Title="Office Romance",
                    Description="A timid statistician falls in love with his strict female boss in this classic romantic comedy."
                },
                new Movie
                {
                    Key=12,
                    Title="Gentlemen of Fortune",
                    Description="A comedy about a kindergarten director who impersonates a criminal to retrieve stolen treasure."
                },
                new Movie
                {
                    Key=13,
                    Title="Diamond Arm",
                    Description="An ordinary man gets entangled with smugglers after unwittingly receiving a cast filled with jewels."
                },
                new Movie
                {
                    Key=14,
                    Title="Ivan Vasilievich: Back to the Future",
                    Description="A scientist's time machine transports a Soviet superintendent and Ivan the Terrible into each other's eras."
                },
                new Movie
                {
                    Key=15,
                    Title="Moscow Does Not Believe in Tears",
                    Description="A drama following three women in Moscow pursuing love and career over two decades."
                },
                new Movie
                {
                    Key=16,
                    Title="Brother",
                    Description="An action film about a young ex-soldier navigating the criminal underworld of St. Petersburg."
                },
                new Movie
                {
                    Key=17,
                    Title="Burnt by the Sun",
                    Description="A colonel's family life unravels during Stalin's Great Purge in this Oscar-winning drama."
                },
                new Movie
                {
                    Key=18,
                    Title="The Cranes Are Flying",
                    Description="A wartime romance depicting the impact of World War II on a young couple in love."
                },
                new Movie
                {
                    Key=19,
                    Title="Ballad of a Soldier",
                    Description="A young soldier's journey home during World War II leads to touching encounters with strangers."
                },
                new Movie
                {
                    Key=20,
                    Title="Solaris",
                    Description="A psychologist is sent to a space station orbiting a mysterious planet to investigate strange phenomena."
                },
                new Movie
                {
                    Key=21,
                    Title="Stalker",
                    Description="A guide leads two men through the forbidden Zone to find a room that grants wishes."
                },
                new Movie
                {
                    Key=22,
                    Title="Hard to Be a God",
                    Description="Scientists observe a planet stuck in medieval times without interfering in its development."
                },
                new Movie
                {
                    Key=23,
                    Title="Mirror",
                    Description="An autobiographical film exploring memories and reflections on life by director Andrei Tarkovsky."
                },
                new Movie
                {
                    Key=24,
                    Title="Come and See",
                    Description="A harrowing portrayal of a Belarusian teenager's experiences during the Nazi occupation."
                },
                new Movie
                {
                    Key=25,
                    Title="Dersu Uzala",
                    Description="An explorer befriends a Siberian hunter in this tale of survival and companionship."
                },
                new Movie
                {
                    Key=26,
                    Title="Dear Comrades!",
                    Description="A historical drama about a Soviet woman's search for her missing daughter during a suppressed uprising."
                },
                new Movie
                {
                    Key=27,
                    Title="Leviathan",
                    Description="A man's battle against corrupt officials trying to take his home in a coastal town."
                },
                new Movie
                {
                    Key=28,
                    Title="Loveless",
                    Description="A couple's bitter divorce takes a turn when their son disappears."
                },
                new Movie
                {
                    Key=29,
                    Title="Attraction",
                    Description="An alien spacecraft crash-lands in Moscow, leading to unforeseen consequences."
                },
                new Movie
                {
                    Key=30,
                    Title="Space Dogs",
                    Description="Animated adventure of Belka and Strelka, the first dogs to return from space."
                },
                new Movie
                {
                    Key=31,
                    Title="Salyut 7",
                    Description="Based on the true story of cosmonauts repairing an unmanned space station."
                },
                new Movie
                {
                    Key=32,
                    Title="The Barber of Siberia",
                    Description="A cadet falls in love with an American woman involved in a steam-powered sawmill project."
                },
                new Movie
                {
                    Key=33,
                    Title="Night Watch",
                    Description="A fantasy thriller depicting the battle between forces of Light and Darkness in modern Moscow."
                },
                new Movie
                {
                    Key=34,
                    Title="Day Watch",
                    Description="The sequel to 'Night Watch,' continuing the story of supernatural forces in conflict."
                },
                new Movie
                {
                    Key=35,
                    Title="12",
                    Description="A Russian adaptation of '12 Angry Men,' where jurors deliberate the fate of a Chechen teenager."
                },
                new Movie
                {
                    Key=36,
                    Title="The Island",
                    Description="A monk's spiritual journey toward redemption on a remote island monastery."
                },
                new Movie
                {
                    Key=37,
                    Title="Hipsters",
                    Description="A musical exploring youth culture in 1950s Soviet Union through jazz and fashion."
                },
                new Movie
                {
                    Key=38,
                    Title="Legend No. 17",
                    Description="A biographical sports drama about ice hockey legend Valeri Kharlamov."
                },
                new Movie
                {
                    Key=39,
                    Title="Ice",
                    Description="A young figure skater's rise and fall, finding love and purpose along the way."
                },
                new Movie
                {
                    Key=40,
                    Title="Union of Salvation",
                    Description="A historical epic about the Decembrist revolt against Tsar Nicholas I."
                },
                new Movie
                {
                    Key=41,
                    Title="Anna Karenina: Vronsky's Story",
                    Description="A retelling of Tolstoy's classic from Count Vronsky's perspective."
                },
                new Movie
                {
                    Key=42,
                    Title="Doctor Lisa",
                    Description="A day in the life of Dr. Elizaveta Glinka, a humanitarian and physician."
                },
                new Movie
                {
                    Key=43,
                    Title="Air Crew",
                    Description="An aviation disaster film about pilots conducting a daring rescue mission."
                },
                new Movie
                {
                    Key=44,
                    Title="Viy",
                    Description="A fantasy film based on Gogol's story of supernatural encounters in a Ukrainian village."
                },
                new Movie
                {
                    Key=45,
                    Title="Alyosha Popovich and Tugarin Zmey",
                    Description="An animated film about the folk hero Alyosha Popovich battling a dragon."
                },
                new Movie
                {
                    Key=46,
                    Title="Dobrynya Nikitich and Zmey Gorynych",
                    Description="An animated tale of the hero Dobrynya rescuing a kidnapped princess."
                },
                new Movie
                {
                    Key=47,
                    Title="Ilya Muromets and Nightingale the Robber",
                    Description="An animated adventure of the hero Ilya Muromets facing off against a notorious thief."
                },
                new Movie
                {
                    Key=48,
                    Title="The Three Bogatyrs and the Shamakhan Queen",
                    Description="The trio of heroes confronts a queen with mystical powers."
                },
                new Movie
                {
                    Key=49,
                    Title="Gagarin: First in Space",
                    Description="A biographical film about Yuri Gagarin, the first human in space."
                }
            };

            var vectorStore = new InMemoryVectorStore();

            var movies = vectorStore.GetCollection<int, Movie>("movies");

            await movies.CreateCollectionIfNotExistsAsync();

            IEmbeddingGenerator<string, Embedding<float>> generator =
                new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "llama3.2"); //all-minilm

            foreach (var movie in movieData)
            {
                movie.Vector = await generator.GenerateEmbeddingVectorAsync(movie.Description);
                await movies.UpsertAsync(movie);
            }

            var query = "A family friendly movie";
            var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);
            var searchOptions = new VectorSearchOptions()
            {
                Top = 2,
                VectorPropertyName = "Vector"
            };

            var results = await movies.VectorizedSearchAsync(queryEmbedding, searchOptions);

            await foreach (var result in results.Results)
            {
                _output.WriteLine($"Title: {result.Record.Title}");
                _output.WriteLine($"Description: {result.Record.Description}");
                _output.WriteLine($"Score: {result.Score}");
                _output.WriteLine("");
            }
        }
    }
}