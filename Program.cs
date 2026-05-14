using AppFilm;
using Domain;
using Presentation;
using Infrostruture.Readers;
using Infrostruture.Api;
using Infrostruture.Loggers;
using AppFilm.Helpers;

var ui = new ConsoleUi();
var logger = new AppLogger(ui);
var helper = new AppHelper(ui);
var httpClient = new HttpClient();
var descriptionService = new ImdbDescriptionService(httpClient, logger, "6e7cf0d1");

var filmById = new Dictionary<long, Film>();

var movieImdbReader = new MovieImdbReader(filmById);
helper.ReaderDiagnostics("MovieImdbReader", () => movieImdbReader.ReadImdb());
filmById = movieImdbReader.MovieCodes;

var raitingReader = new RaitingReader(filmById);
helper.ReaderDiagnostics("RaitingReader", () => raitingReader.ReadRaiting());

var peopleNamesReader = new PeopleNamesReader(filmById);
helper.ReaderDiagnostics("PeopleNamesReader", () => peopleNamesReader.ReadNames());

var peopleRolesReader = new PeopleRolesReader(filmById, peopleNamesReader.PersonById);
helper.ReaderDiagnostics("PeopleRolesReader", () => peopleRolesReader.ReadRoles());

var imbdToMovieCode = new ImbdToMovieCode(filmById);
helper.ReaderDiagnostics("ImbdToMovieCode", () => imbdToMovieCode.Convert());

var tagCodeReader = new TagCodeReader();
helper.ReaderDiagnostics("TagCodeReader", () => tagCodeReader.ReadTags());

var tagsApplyer = new TagsApplyer(tagCodeReader.TagCode, filmById, imbdToMovieCode.CodeConvertor);
helper.ReaderDiagnostics("TagsApplyer", () => tagsApplyer.ApplyTags());

var films = new HashSet<Film>(filmById.Values);

var app = new App(ui, logger, descriptionService, helper, films);
helper.ReaderDiagnostics("BuildIndexes", () => app.BuildIndexes());
app.Run();
