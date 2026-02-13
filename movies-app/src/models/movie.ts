import { MediaType } from "./media-type";
import { MovieFormat } from "./movie-format";

export interface Movie {
    Title: string;
    Description: string;
    Id: number;
    Genre: string;
    Criterion: boolean | undefined;
    CriterionNumber: number | undefined;
    MovieMediaType: MediaType | undefined;
    MovieFormat: MovieFormat | undefined;
    Year: number;
    Favourite: boolean | undefined;
}
