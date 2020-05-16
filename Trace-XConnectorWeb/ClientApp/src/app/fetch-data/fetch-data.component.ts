import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-fetch-data',
    templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
    public forecasts: WeatherForecast[];
    baseUrl: string;
    http: HttpClient;

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.http = http;
        this.baseUrl = baseUrl;

        //http.get<WeatherForecast[]>(baseUrl + 'weatherforecast').subscribe(result => {
        //  this.forecasts = result;
        //}, error => console.error(error));
    }


    public startOnce() {

      this.http.get<WeatherForecast[]>(this.baseUrl + 'weatherforecast/1').subscribe(result => {
        this.forecasts = result;
      }, error => console.error(error));

    }


    public start() {

        this.http.get<WeatherForecast[]>(this.baseUrl + 'weatherforecast').subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));

    }


    public stop() {

        this.http.get<WeatherForecast[]>(this.baseUrl + 'weatherforecast').subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));

    }
}

interface WeatherForecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}
