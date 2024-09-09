const express = require('express');
const app = express();
const port = 3000;

// 中间件：解析 JSON 请求体
app.use(express.json());

app.post('/api/receive-data', (req, res) => {
    // 获取请求体中的数据
    const { data, state, num, word } = req.body;

    // 打印接收到的数据
    console.log('Received data:', req.body);

    // 返回成功响应
    res.status(200).send('Data received successfully');
});

app.listen(port, () => {
    console.log(`Server is running on http://localhost:${port}`);
});
