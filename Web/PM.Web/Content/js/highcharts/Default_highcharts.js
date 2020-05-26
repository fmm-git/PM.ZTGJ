//项目收入合同台账
//y轴的金额会取最大金额，作为分组依据即y轴分界线为自动生成
Highcharts.chart('ProjectConMoney', {
    chart: {
        type: 'column'//报表类型
    },
    title: {
        text: '项目收入合同台账'//报表标题
    },
    xAxis: {
        categories: ['Apples', 'Oranges', 'Pears', 'Grapes', 'Bananas']//x轴标题
    },
    credits: {
        enabled: false//是否允许编辑
    },
    yAxis: {
        title: {
            text: '金额(万元)',                // y 轴标题
            min: 0,
            max: 360,
            tickInterval: 30
        }
    },
    series: [{//各不同类型的金额
        name: '收入合同金额',
        data: [10000, 10000, 10000, 10000, 10000]
    }, {
        name: '结算金额',
        data: [10000, 10000, 10000, 10000, 10000]
    }, {
        name: '收款金额',
        data: [10000, 10000, 10000, 10000, 10000]
    }, {
        name: '开票金额',
        data: [10000, 10000, 10000, 10000, 10000]
    }]
});

//项目数量
Highcharts.chart('ProjectNum', {
    chart: {
        type: 'column'//类型
    },
    title: {
        text: '项目数量'//标题
    },
    xAxis: {
        categories: ['Apples', 'Oranges', 'Pears', 'Grapes', 'Bananas']//x-轴列名
    },
    yAxis: {
        min: 0,
        title: {
            text: '项目状态所占比例(%)'//y-轴标题
        }
    },
    tooltip: {
        pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.y}</b> ({point.percentage:.0f}%)<br/>',//给每个数值加上%
        shared: true
    },
    plotOptions: {
        column: {
            stacking: 'percent'
        }
    },
    series: [{//具体的数值
        name: '项目未开工',
        data: [1, 3, 4, 7, 2]
    }, {
        name: '项目已开工',
        data: [2, 2, 3, 2, 1]
    }, {
        name: '项目已完工',
        data: [3, 4, 4, 2, 5]
    }, {
        name: '项目已停工',
        data: [3, 4, 4, 2, 5]
    }, {
        name: '项目已验收',
        data: [3, 4, 4, 2, 5]
    }, {
        name: '项目已转接',
        data: [3, 4, 4, 2, 5]
    }, {
        name: '项目已取消',
        data: [3, 4, 4, 2, 5]
    }]
});