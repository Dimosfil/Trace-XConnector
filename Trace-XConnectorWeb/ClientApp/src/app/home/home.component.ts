//import { Component } from '@angular/core';

//@Component({
//  selector: 'app-home',
//  templateUrl: './home.component.html',
//})
//export class HomeComponent {
//}

import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
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
    

    public getInfo() {

      //this.http.get<WeatherForecast[]>(this.baseUrl + 'weatherforecast').subscribe(result => {
      //  this.forecasts = result;
      //}, error => console.error(error));


        this.getWithOutId().subscribe(result => {
        this.forecasts = result;
      }, error => console.error(error));
    }

    public startOnce() {

        //this.http.get<WeatherForecast[]>(this.baseUrl + 'weatherforecast').subscribe(result => {
        //  this.forecasts = result;
        //}, error => console.error(error));


        this.getWithId(2).subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));
    }

    getWithOutId() {
      var fullUrl = this.GetWeatherForecastString();
      return this.http.get<WeatherForecast[]>(fullUrl);
    }

    getWithId(commandId: number) {
        var fullUrl = this.GetWeatherForecastString() + '/' + commandId;
        return this.http.get<WeatherForecast[]>(fullUrl);
    }

    GetWeatherForecastString() {
        var fullUrl = this.baseUrl + 'api/' + 'Prosalex';
        return fullUrl;
    }


    public start() {

        this.getWithId(1).subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));

    }


    public stop() {

        this.getWithId(0).subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));
    }

    public reject() {

        this.getWithId(-1).subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));

    }

    public uidrequestsGet() {

        this.getuidString(true, 'RussianCRPT').subscribe(result => {
            this.forecasts = result;
        }, error => console.error(error));

    }
    getuidString(isSync: boolean, format: string) {
        var fullUrl = this.GetUidrequestsString() + '?isSync=' + isSync + '&format=' + format;
        return this.http.get<WeatherForecast[]>(fullUrl);
    }

    GetUidrequestsString() {
        var fullUrl = this.baseUrl + 'api/v2/' + 'uidrequests';
        return fullUrl;
    }

    SGTINPost() {

        var body = {
            UIDRequestTypeKey: "04607035391972_CRPT",
            Name: "SampleUIDRequestName",
        }

        this.getuidStringPost(true, 'RussianCRPT', body).subscribe(result => {
            var array = new Array<WeatherForecast>();
            var item = new WeatherForecast();
            item.summary = result.toString();
            array.push(item);

            this.forecasts = array;
        }, error => console.error(error));
    }

    SSCCPostCase() {

        var body = {
            UIDRequestTypeKey: "1+4607035",
            Name: "SampleUIDRequestName",
        }

        this.getuidStringPost(true, 'DetailURI', body).subscribe(result => {
            var array = new Array<WeatherForecast>();
            var item = new WeatherForecast();
            item.summary = result.toString();
            array.push(item);

            this.forecasts = array;
        }, error => console.error(error));
    }

    SSCCPostPallet() {

        var body = {
            UIDRequestTypeKey: "2+4607035",
            Name: "SampleUIDRequestName",
        }
        this.getuidStringPost(true, 'DetailURI', body).subscribe(result => {
            var array = new Array<WeatherForecast>();
            var item = new WeatherForecast();
            item.summary = result.toString();
            array.push(item);

            this.forecasts = array;
        }, error => console.error(error));
    }

    getuidStringPost(isSync: boolean, format: string, body: any) {
        var fullUrl = this.GetUidrequestsString() + '?isSync=' + isSync + '&format=' + format;

        //var options: {
        //    headers?: HttpHeaders | {
        //        "HTTP/1.1 200 OK",
        //        'Content-type:application/json;charset=utf-8',
        //    },
        //}

        const headerDict = {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Access-Control-Allow-Headers': 'Content-Type',
        }

        const requestOptions: any = { headers: headerDict };

        return this.http.post<string>(fullUrl, body, requestOptions);
    }

}

class WeatherForecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

