(function () {
    'use strict';

    var SECTIONS = [
        { endpoint: 'RecentMovies', title: 'Recientemente estrenado en Pel\u00edculas' },
        { endpoint: 'RecentSeries', title: 'Recientemente estrenado en Series' },
        { endpoint: 'TopMovies', title: 'Top Pel\u00edculas' },
        { endpoint: 'TopSeries', title: 'Top Series' }
    ];

    var CONTAINER_ID = 'topjellyfin-sections';
    var rendered = false;
    var currentPath = '';

    function getApiHeaders() {
        var token = null;
        try {
            if (window.ApiClient && window.ApiClient.accessToken && window.ApiClient.accessToken()) {
                token = window.ApiClient.accessToken();
            }
        } catch (e) {
            try {
                var creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
                if (creds.Servers && creds.Servers.length > 0) {
                    token = creds.Servers[0].AccessToken;
                }
            } catch (e2) { /* ignore */ }
        }

        var headers = { 'Content-Type': 'application/json' };
        if (token) {
            headers['Authorization'] = 'MediaBrowser Token="' + token + '"';
        }
        return headers;
    }

    function getServerUrl() {
        try {
            if (window.ApiClient && window.ApiClient.serverAddress && window.ApiClient.serverAddress()) {
                return window.ApiClient.serverAddress();
            }
        } catch (e) { /* ignore */ }
        return '';
    }

    function fetchSection(endpoint) {
        var serverUrl = getServerUrl();
        var url = serverUrl + '/TopJellyfin/' + endpoint;
        return fetch(url, { headers: getApiHeaders() })
            .then(function (response) {
                if (!response.ok) return [];
                return response.json();
            })
            .catch(function (e) {
                console.warn('TopJellyfin: Error fetching ' + endpoint, e);
                return [];
            });
    }

    function createCard(item) {
        var serverUrl = getServerUrl();
        var detailUrl = '#!/details?id=' + item.Id;

        var card = document.createElement('div');
        card.className = 'card overflowPortraitCard';
        card.style.cssText = 'min-width: 140px; width: 140px; margin-right: 6px; flex-shrink: 0;';

        var link = document.createElement('a');
        link.href = detailUrl;
        link.className = 'cardBox';
        link.style.cssText = 'display:block; text-decoration:none;';

        var scalable = document.createElement('div');
        scalable.className = 'cardScalable';
        scalable.style.cssText = 'position:relative; padding-bottom:150%;';

        var padder = document.createElement('div');
        padder.className = 'cardPadder';
        scalable.appendChild(padder);

        var content = document.createElement('div');
        content.className = 'cardContent';
        content.style.cssText = 'position:absolute; inset:0;';

        var imageContainer = document.createElement('div');
        imageContainer.className = 'cardImageContainer coveredImage';

        if (item.HasPrimaryImage) {
            var imgUrl = serverUrl + '/Items/' + encodeURIComponent(item.Id) + '/Images/Primary?fillHeight=350&fillWidth=230&quality=96';
            imageContainer.style.cssText = 'background-size:cover; background-position:center; height:100%; border-radius:4px;';
            imageContainer.style.backgroundImage = 'url(' + imgUrl + ')';
        } else {
            imageContainer.style.cssText = 'background:#333; height:100%; display:flex; align-items:center; justify-content:center; border-radius:4px;';
            var fallbackText = document.createElement('span');
            fallbackText.style.cssText = 'color:#888; font-size:12px;';
            fallbackText.textContent = item.Name;
            imageContainer.appendChild(fallbackText);
        }

        content.appendChild(imageContainer);
        scalable.appendChild(content);
        link.appendChild(scalable);

        var footer = document.createElement('div');
        footer.className = 'cardFooter';
        footer.style.cssText = 'padding:4px 0;';

        var nameText = document.createElement('div');
        nameText.className = 'cardText';
        nameText.style.cssText = 'font-size:13px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; color:#ddd;';
        nameText.textContent = item.Name;
        footer.appendChild(nameText);

        if (item.ProductionYear) {
            var yearText = document.createElement('div');
            yearText.className = 'cardText cardTextSecondary';
            yearText.style.cssText = 'font-size:11px; color:#999;';
            yearText.textContent = item.ProductionYear;
            footer.appendChild(yearText);
        }

        link.appendChild(footer);
        card.appendChild(link);
        return card;
    }

    function createSection(title, items) {
        if (!items || items.length === 0) return null;

        var section = document.createElement('div');
        section.className = 'verticalSection topjellyfin-section';
        section.style.cssText = 'margin-bottom: 24px;';

        var header = document.createElement('div');
        header.className = 'sectionTitleContainer sectionTitleContainer-cards';
        header.style.cssText = 'display:flex; align-items:center; padding:0 0 8px 0;';

        var h2 = document.createElement('h2');
        h2.className = 'sectionTitle sectionTitle-cards';
        h2.style.cssText = 'margin:0; font-size:1.4em; font-weight:600; color:#fff;';
        h2.textContent = title;
        header.appendChild(h2);
        section.appendChild(header);

        var scroller = document.createElement('div');
        scroller.className = 'itemsContainer scrollSlider';
        scroller.style.cssText = 'display:flex; overflow-x:auto; overflow-y:hidden; scroll-behavior:smooth; padding-bottom:8px; -webkit-overflow-scrolling:touch; scrollbar-width:none;';

        items.forEach(function (item) {
            scroller.appendChild(createCard(item));
        });

        section.appendChild(scroller);
        return section;
    }

    function renderSections() {
        if (rendered) return;
        rendered = true;

        var container = document.querySelector('.homeSectionsContainer')
            || document.querySelector('#homeTab .sections')
            || document.querySelector('#homeTab')
            || document.querySelector('.view-home .sections');

        if (!container) {
            rendered = false;
            return;
        }

        if (document.getElementById(CONTAINER_ID)) return;

        var wrapper = document.createElement('div');
        wrapper.id = CONTAINER_ID;
        wrapper.style.cssText = 'padding: 0 3.3% 0;';

        Promise.all(SECTIONS.map(function (s) {
            return fetchSection(s.endpoint).then(function (items) {
                return { title: s.title, items: items };
            });
        })).then(function (results) {
            var hasContent = false;
            results.forEach(function (r) {
                var sectionEl = createSection(r.title, r.items);
                if (sectionEl) {
                    wrapper.appendChild(sectionEl);
                    hasContent = true;
                }
            });

            if (hasContent && container.parentNode) {
                container.insertBefore(wrapper, container.firstChild);
            } else {
                rendered = false;
            }
        }).catch(function () {
            rendered = false;
        });
    }

    function cleanup() {
        var existing = document.getElementById(CONTAINER_ID);
        if (existing && existing.parentNode) {
            existing.parentNode.removeChild(existing);
        }
        rendered = false;
    }

    function isHomePage() {
        var path = window.location.hash || window.location.pathname;
        return path === '#!/home.html' || path === '#/' || path === '' ||
               path === '#!/' || path.indexOf('/home') !== -1;
    }

    function checkAndRender() {
        var path = window.location.hash || window.location.pathname;
        if (path !== currentPath) {
            currentPath = path;
            if (!isHomePage()) {
                cleanup();
                return;
            }
        }

        if (isHomePage() && !rendered) {
            setTimeout(function () {
                if (isHomePage()) {
                    renderSections();
                }
            }, 1500);
        }
    }

    function init() {
        setTimeout(checkAndRender, 2000);

        window.addEventListener('hashchange', function () {
            cleanup();
            setTimeout(checkAndRender, 1500);
        });

        var observer = new MutationObserver(function () {
            if (isHomePage() && !rendered && !document.getElementById(CONTAINER_ID)) {
                checkAndRender();
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
