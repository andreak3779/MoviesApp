export enum MovieFormat {
     Movie=1,
     TVSeries=2,
     BoxSet=3
}

// Backward-compatible alias for the previous enum member name
export const TVseries = MovieFormat.TVSeries;
