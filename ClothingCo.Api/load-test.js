import http from 'k6/http';
import { check } from 'k6';
import { sleep } from 'k6';

export const options = {
    vus: 5,
    duration: '4h'
};



export default function() {
    let products = [
        "Hat",
        "Hat",
        "Hat",
        "T-Shirt",
        "Jeans",
        "Jeans",
        "Socks",
        "Socks",
        "Socks",
        "Hoodie"
    ];
    
    let res = http.post('http://localhost:5033/orders', JSON.stringify({        
        "productName": products[Math.floor(Math.random()*products.length)],
        "quantity": randomIntFromInterval(1, 200)
    }), {
        headers: { 'Content-Type': 'application/json' },
    });
    check(res, {
        'is status 200': (r) => r.status === 200,
    });
    sleep(1);
}

function randomIntFromInterval(min, max) { // min and max included 
    return Math.floor(Math.random() * (max - min + 1) + min)
}